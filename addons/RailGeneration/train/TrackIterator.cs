namespace RailGeneration;

using Godot;
using System;
using RailGeneration.Common;

using static BasisMath;

public class TrackIterator
{
    public event Action Derailed;
    
    private TrackPiece _track;
    private float _s; // current arc length starting from _track.InitialPose
    private Terminal _facing;
    
    public TrackPiece Track => _track;
    public float S => _s;
    public Terminal Facing => _facing;
    public bool IsDerailed => _track == null;
    
    public TrackIterator()
    {
        _track = null;
        _s = 0;
        _facing = Terminal.Final;
    }

    public void Rerail(TrackPiece track, float s, Terminal facing)
    {
        _track = track;
        _s = s;
        _facing = facing;
    }

    public void Copy(TrackIterator iter)
    {
        _track = iter._track;
        _s = iter._s;
        _facing = iter._facing;
    }

    /// <summary>
    /// returns pose in global space
    /// </summary>
    public Pose GetPose()
    {
        if(_track == null) { throw new InvalidOperationException("derailed TrackIterator"); }

        Pose localPose = _track.C(_s);

        Vector3 pos = localPose.Position;
        Basis axes = localPose.Axes;

        if(_facing == Terminal.Initial) { axes = Reverse(axes); }

        pos = _track.GlobalTransform * pos;
        axes = _track.GlobalBasis * axes;

        return new Pose(pos, axes, localPose.Bank, localPose.R);
    }

    public void Move(float ds)
    {
        while(_track != null)
        {
            _s += (_facing == Terminal.Final)? ds: -ds;

            if(_s < 0) // go to prev
            {
                // remaining ds (keep sign of ds)
                if(ds > 0) { ds = -_s; }
                else { ds = _s; }

                // derailed
                if(_track.Prev == null)
                {
                    Derailed?.Invoke();
                    _track = null;
                    break;
                }

                // update s
                if(_track.PrevTerminal == Terminal.Initial) { _s = 0; }
                else { _s = _track.Prev.TotalLength; }

                // update facing
                if(_track.PrevTerminal == Terminal.Initial) { _facing = (_facing == Terminal.Initial)? Terminal.Final: Terminal.Initial; }

                // update track
                _track = _track.Prev;
            }
            else if(_s > _track.TotalLength) // go to next
            {
                // remaining ds (keep sign of ds)
                if(ds > 0) { ds = _s - _track.TotalLength; }
                else { ds = _track.TotalLength - _s; }

                // derailed
                if(_track.Next == null)
                {
                    Derailed?.Invoke();
                    _track = null;
                    break;
                }

                // update s
                if(_track.NextTerminal == Terminal.Initial) { _s = 0; }
                else { _s = _track.Next.TotalLength; }

                // update facing
                if(_track.NextTerminal == Terminal.Final) { _facing = (_facing == Terminal.Initial)? Terminal.Final: Terminal.Initial; }

                // update track
                _track = _track.Next;
            }
            else
            {
                break;
            }
        }
    }
}
