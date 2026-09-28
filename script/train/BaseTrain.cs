using Godot;
using System;

using static BasisMath;

public class BaseTrain
{
    public event Action EndOfLineReached;
    
    private TrackPiece _track = null;
    private float _s; // current arc length starting from _track.InitialPose
    private Terminal _facing = Terminal.None;

    public BaseTrain(TrackPiece track, float s, Terminal facing)
    {
        _track = track;
        _s = s;
        _facing = facing;
    }

    /// <summary>
    /// returns pose in parent space of _track
    /// </summary>
    public Pose GetPose()
    {
        Pose localPose = _track.C(_s);

        Vector3 pos = localPose.Position;
        Basis axes = localPose.Axes;

        if(_facing == Terminal.Initial) { axes = Reverse(axes); }

        pos = _track.Transform * pos;
        axes = _track.Basis * axes;

        return new Pose(pos, axes, localPose.Bank, localPose.R);
    }

    public void Move(float ds)
    {
        while(_track != null)
        {
            _s += (_facing == Terminal.Final)? ds: -ds;

            // go to prev
            if(_s < 0 && _track.Prev != null)
            {
                // remaining ds (keep sign of ds)
                if(ds > 0) { ds = -_s; }
                else { ds = _s; }

                // update s
                if(_track.PrevTerminal == Terminal.Initial) { _s = 0; }
                else { _s = _track.Prev.TotalLength; }

                // update facing
                if(_track.PrevTerminal == Terminal.Initial) { _facing = (_facing == Terminal.Initial)? Terminal.Final: Terminal.Initial; }

                _track = _track.Prev;
            }
            // go to next
            else if(_s > _track.TotalLength && _track.Next != null)
            {
                // remaining ds (keep sign of ds)
                if(ds > 0) { ds = _s - _track.TotalLength; }
                else { ds = _track.TotalLength - _s; }

                // update s
                if(_track.NextTerminal == Terminal.Initial) { _s = 0; }
                else { _s = _track.Next.TotalLength; }

                // update facing
                if(_track.NextTerminal == Terminal.Final) { _facing = (_facing == Terminal.Initial)? Terminal.Final: Terminal.Initial; }

                _track = _track.Next;
            }
            else
            {
                break;
            }
        }

        if(_track == null) { EndOfLineReached?.Invoke(); }
    }
}
