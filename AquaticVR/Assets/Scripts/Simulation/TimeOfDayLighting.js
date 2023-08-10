import System;
var date = DateTime.Now;
var timeDisplay : GUIText;

function Start() {
    InvokeRepeating("Increment", 1.0, 1.0);
}
function Update () {
    var seconds : float = date.TimeOfDay.Ticks / 10000000;
    transform.rotation = Quaternion.LookRotation(Vector3.up);
    transform.rotation *= Quaternion.AngleAxis(seconds/86400*360,Vector3.down);
    if (timeDisplay) timeDisplay.text = date.ToString("f");
}

function Increment() {
    date += TimeSpan(0,0,0, 1);
}