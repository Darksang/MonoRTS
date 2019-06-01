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
        PathFollowing,
        Pursue,
        Wander
    }
}
