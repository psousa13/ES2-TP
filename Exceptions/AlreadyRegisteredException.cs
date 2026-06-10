namespace TalentosIT.Web.Exceptions
{
    public class AlreadyRegisteredException : Exception
    {
        public AlreadyRegisteredException()
        {}

        public AlreadyRegisteredException(string? message) : base(message)
        {}

        public AlreadyRegisteredException(string? message, Exception? innerException) : base(message, innerException)
        {}
    }
}