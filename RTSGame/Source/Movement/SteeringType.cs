namespace RTSGame {

    public enum SteeringType {
        // Basic Steerings
        Align,
        AntiAlign,
        Arrive,
        Flee,
        Seek,
        VelocityMatching,

        // Delegate Steerings
        Evade,
        Face,
        LookWhereYouGoing,
        PathFollowing,
        Pursue,
        Wander,
        MoveToPosition,

        // Group Steerings
        Alignment,
        Cohesion,
        Separation
    }
}
