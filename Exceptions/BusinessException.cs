public class BusinessException : Exception 
{
    public string Property { get; set; }
    public BusinessException(string property, string message): base(message) {
        Property = property;
    }
}