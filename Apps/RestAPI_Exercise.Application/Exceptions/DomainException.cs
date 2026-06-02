namespace RestAPI_Exercise.Application.Exceptions;
/// <summary>
/// 業務ルール違反を表す例外クラス
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) 
    : base(message, innerException) { }
}