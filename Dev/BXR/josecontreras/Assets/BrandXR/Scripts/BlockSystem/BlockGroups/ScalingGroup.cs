/* ScalingGroup.cs
 * 
 * Inherits from BlockGroup.cs.
 * 
 * Controls childed ScalingGroup.cs child components and Block.cs child components 
 * scaling within a given view, and provides convenience options 
 * for creating new ScalingGroup BlockGroups and scaled/scalable blocks in the editor
 * 
 * This is primarily used when building an XR experience to allow child blocks to behave responsively
 * as the customer adds new blocks to the view.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class ScalingGroup : BlockGroup
    {
        [SerializeField]
        public float padding = 100; // in Unity units

        // if these are both at 100, then it treats it like a 100x100 grid with each grid point being a "percentage point"
        private int rowCount = 100;
        private int colCount = 100;

        private float cellWidth = -1; // the actual width of a single cell of the grid relative to the actual width of this object.
        private float cellHeight = -1; // the actual height of a single cell of the grid relative to the actual height of this object.

        private RectTransform selfRect;

        // the current and previous values for the width/height of this object. Used to know when to adjust the size of individual cells.
        private float currWidth;
        private float currHeight;
        private float prevWidth = -1;
        private float prevHeight = -1;

        private float xPos;
        private float yPos;

#if UNITY_EDITOR

        public override bool ShouldShowBlockButton()
        {
            return false;
        }

        public override bool ShouldShowAddNestedBlockGroupButtonXR()
        {
            return false;
        }

        public override bool ShouldShowAddNestedBlockGroupButton3D()
        {
            return true;
        }

        public void Update()
        {
            if (selfRect == null)
            {
                selfRect = gameObject.GetComponent<RectTransform>();
            }

            if (selfRect != null)
            {
                // get the current width/height
                currWidth = selfRect.rect.width - (padding * 2);
                currHeight = selfRect.rect.height - (padding * 2);

                // if the previous values are empty, then set them
                if (prevWidth == -1)
                {
                    prevWidth = currWidth;
                }
                
                if (prevHeight == -1)
                {
                    prevHeight = currHeight;
                }

                xPos = selfRect.position.x - (currWidth * selfRect.pivot.x);
                yPos = selfRect.position.y - (currHeight * selfRect.pivot.y);

                // if the current values and previous values don't match then update them, the cell size, and adjust children to resize them to their new space.
                if (prevWidth != currWidth || prevHeight != currHeight)
                {
                    prevWidth = currWidth;
                    prevHeight = currHeight;
                    cellWidth = (currWidth - ((colCount - 1) * padding)) / colCount;
                    cellHeight = (currHeight - ((rowCount - 1) * padding)) / rowCount;
                    AdjustChildren();
                }
            }

            base.Update();
        }

        // This is the method to resize the actual children objects to fit within their new space
        public void AdjustChildren()
        {
            if (nestedBlockGroups != null && nestedBlockGroups.Count > 0)
            {
                for (int i = 0; i < nestedBlockGroups.Count; i++)
                {
                    BlockGroup member = nestedBlockGroups[i];

                    float newX = (cellWidth * (member.xPosition - 1)) + (member.xPosition * padding);
                    float newY = (cellHeight * (member.yPosition - 1)) + (member.yPosition * padding);
                    
                    float newW = (cellWidth * member.hPercent) + (padding * (member.hPercent - 1));
                    float newH = (cellHeight * member.vPercent) + (padding * (member.vPercent - 1));
                    
                    RearrangeChildren();

                    RectTransform[] rts = nestedBlockGroups[i].GetComponentsInChildren<RectTransform>();
                    if (rts != null && rts.Length > 0)
                    {
                        foreach (RectTransform r in rts)
                        {
                            if (r != null)
                            {
                                // set the actual worldspace position based on the position of the parent. 
                                // This math sort of works but has/causes problems.
                                // The biggest issue is that I didn't account for the pivot of the child and so everything is off by half the size of the object with the default RectTransform pivot of 0.5
                                // There might be other issues with multiple Blocks within a BlockGroup. I haven't tested it with that but it may adjust all the child Blocks of a BlockGroup to be the full size of the BlockGroup.
                                // Lastly, this relies on the child objects having a RectTransform component. 
                                // You'd have to modify it to use scaling if you didn't want that and scaling has issues because of how it scales from the origin of the object and so as you scaled, you'd have to translate the object to keep it lined up.
                                float actualX = xPos + newX;
                                float actualY = yPos + newY;
                                float actualZ = 0; // to keep it on the 2D plane
                                Vector3 newPos = new Vector3(actualX, actualY, actualZ);
                                r.SetPositionAndRotation(newPos, r.transform.rotation);
                                r.sizeDelta = new Vector2(newW, newH);
                            }
                        }
                    }
                }
            }
        }

        public Vector2 RearrangeChildren()
        {
            // put code here to rearrange pieces as other pieces move
            return new Vector2();
        }

#endif

    } //END Class

    public class ScalingGroupMember : MonoBehaviour
    {
        public GameObject member; // the object to scale

        public int ID = 0;

        public int hPercent; // the whole number horizontal percentage of space this object should take up
        public int vPercent; // the whole number vertical percentage of space this object should take up
        public int xPosition; // the whole number grid x position in a 100x100 grid that this object should start in
        public int yPosition; // the whole number grid y position in a 100x100 grid that this object should start in

        private int prevH; // previous h value. Only update if the current and prev don't match to avoid killing the update process.
        private int prevV; // previous v value. Only update if the current and prev don't match to avoid killing the update process.
        private int prevX; // previous x value. Only update if the current and prev don't match to avoid killing the update process.
        private int prevY; // previous y value. Only update if the current and prev don't match to avoid killing the update process.

        [ExecuteInEditMode]
        public void Update()
        {
            if (prevH != hPercent || prevV != vPercent || xPosition != prevX || yPosition != prevY)
            {
                // update the "prev" values
                prevH = hPercent;
                prevV = vPercent;
                prevX = xPosition;
                prevY = yPosition;
            }
        }

        public void AdjustMember(int h, int v, int x, int y)
        {
            if (h != hPercent) { hPercent = h; }
            if (v != vPercent) { vPercent = v; }
            if (x != xPosition) { xPosition = x; }
            if (y != yPosition) { yPosition = y; }
        }

    } //END Class
    
} //END Namespace