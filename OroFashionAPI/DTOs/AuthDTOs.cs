namespace OroFashionAPI.DTOs;

public class RegisterDTO
{
    public string FullName {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;
    public string Phone {get; set;} = string.Empty;
    public string Address {get; set;} = string.Empty;
}
public class LoginDTO
{
    public string Email {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;
}
public class AuthResponseDTO
{
    public string Token {get; set;} = string.Empty;
    public string UserId {get; set;} = string.Empty;
    public string FullName {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;
    public string Phone {get; set;} = string.Empty;
    public string Address {get; set;} = string.Empty;
    public string Role {get; set;} = string.Empty;
}
public class UpdateProfileDTO
{
    public string FullName {get; set;} = string.Empty;
    public string Phone {get; set;} = string.Empty;
    public string Address {get; set;} = string.Empty;
}