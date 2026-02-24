namespace Project.Domain.Entities
{
    /// <summary>
    /// Baseline audit metadata for traceability (NIS2/ISO-aligned evidence).
    /// Store UTC timestamps only (ISO 8601) and capture actor identifiers.
    /// </summary>
    public abstract class AuditableEntity
    {
        /// <summary>
        /// Created on UTC timestamp.
        /// </summary>
        public DateTimeOffset CreatedOnUtc { get; protected set; }

        /// <summary>
        /// Actor identifier of the creator.
        /// </summary>
        public string CreatedBy { get; protected set; } = "system";

        /// <summary>
        /// Last modified on UTC timestamp.
        /// </summary>
        public DateTimeOffset? ModifiedOnUtc { get; protected set; }
        /// <summary>
        /// Actor identifier of the last modifier.
        /// </summary>
        public string? ModifiedBy { get; protected set; }

        /// <summary>
        /// Tracks the version of the entity for concurrency and state management purposes.
        /// </summary>
        public long Version { get; protected set; } = 0;

        /// <summary>
        /// Marks the entity as created by setting the creation timestamp, the actor identifier,
        /// and initializing the version to 1.
        /// </summary>
        /// <param name="time">The instance of <c>TimeProvider</c> used to retrieve the current UTC timestamp.</param>
        /// <param name="actor">The identifier of the actor who created the entity.</param>
        protected void MarkCreated(TimeProvider time, string actor)
        {
            CreatedOnUtc = time.GetUtcNow();
            CreatedBy = actor;
            Version = 1;
        }

        /// <summary>
        /// Updates the modification metadata for the entity by setting the modification timestamp,
        /// the actor identifier, and incrementing the version number.
        /// </summary>
        /// <param name="time">The instance of <c>TimeProvider</c> used to retrieve the current UTC timestamp.</param>
        /// <param name="actor">The identifier of the actor who modified the entity.</param>
        protected void MarkModified(TimeProvider time, string actor)
        {
            ModifiedOnUtc = time.GetUtcNow();
            ModifiedBy = actor;
            Version++;
        }
    }
}