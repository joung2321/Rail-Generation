using Godot;

public partial class TrainCar : Node3D
{
    [Export] private Node3D _frontBogie;
    [Export] private Node3D _rearBogie;

    private bool _positionUpdated;
    private float _halfL_m; // half wheelbase in a car [m]
    private TrackIterator _ti_center;
    private TrackIterator _ti_front;
    private TrackIterator _ti_rear;

    public void UpdatePosition(TrackIterator iterator)
    {
        _ti_center.Copy(iterator);
        _positionUpdated = true;
    }

    public override void _Ready()
    {
        _halfL_m = _rearBogie.Position.Z;

        _ti_center = new TrackIterator();
        _ti_front = new TrackIterator();
        _ti_rear = new TrackIterator();
    }

    public override void _Process(double delta)
    {
        if(!_positionUpdated) { return; }

        _ti_front.Copy(_ti_center);
        _ti_rear.Copy(_ti_center);

        _ti_front.Move(_halfL_m);
        _ti_rear.Move(-_halfL_m);

        Pose frontPose = _ti_front.GetPose();
        Pose rearPose = _ti_rear.GetPose();

        // carbody
        Position = (frontPose.Position + rearPose.Position) / 2;
        LookAt(frontPose.Position, _ti_center.GetPose().Axes.Column1);

        // front bogie
        _frontBogie.GlobalPosition = frontPose.Position;
        _frontBogie.GlobalBasis = frontPose.Axes;

        // rear bogie
        _rearBogie.GlobalPosition = rearPose.Position;
        _rearBogie.GlobalBasis = rearPose.Axes;
        
        _positionUpdated = false;
    }
}
