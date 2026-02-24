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
    }
}