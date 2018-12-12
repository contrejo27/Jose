// Callback.cs v1.0 by Magnus Wolffelt, magnus.wolffelt@gmail.com
//
// Delegates used in Messenger.cs.
// http://wiki.unity3d.com/index.php?title=CSharpMessenger_Extended
// NOTES: This is a class downloaded from the Unify wiki, refer to the URL above for proper documentation

public delegate void Callback();
public delegate void Callback<T>(T arg1);
public delegate void Callback<T, U>(T arg1, U arg2);
public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);
