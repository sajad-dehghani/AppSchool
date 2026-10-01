namespace NovinApp.Shared.Enums
{
    public enum GenderType
    {
        Unknown = 0,
        Male = 1,
        Female = 2
    }

    public enum ImportStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        PartialFailure = 4
    }

    public enum StudyTaskStatus
    {
        Planned = 0,
        InProgress = 1,
        Completed = 2,
        Missed = 3
    }

    public enum DuplicateHandlingMode
    {
        Skip = 1,
        Update = 2,
        Error = 3
    }
}
