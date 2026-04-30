namespace TalentosIT.Web.Exceptions
{
    public class NoSkillsException : Exception
    {
        public NoSkillsException()
        {}

        public NoSkillsException(string? message) : base(message)
        {}

        public NoSkillsException(string? message, Exception? innerException) : base(message, innerException)
        {}
    }
}