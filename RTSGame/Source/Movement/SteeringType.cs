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
        ObstacleAvoidance,
        PathFollowing,
        Pursue,
        Wander,

        // Group Steerings
        Alignment,
        Cohesion,
        Separation
    }
}
