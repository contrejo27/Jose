package com.nanoequipment.mergeunitywithandroidtest;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;

//import com.gel.helloworld.UnityPlayerActivity;
import com.unity3d.player.UnityPlayerActivity;

        //nanoequipment.mergeunitywithandroidtest.MainActivity;


/**
 * Need to create a holder activity of Unity to solve problem when pressing back button
 */

public class UnityHolderActivity extends AppCompatActivity {


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_next);

        Intent intent = new Intent(this, UnityPlayerActivity.class);
        intent.putExtra("arguments", "data from android");
        startActivity(intent);

    }
}
