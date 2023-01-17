using System.Security.Cryptography;
namespace SplitBackApi.Helper;
public static class InvitationCodeGenerator
{
  public static string GenerateInvitationCode()
  {
    using (var rng = RandomNumberGenerator.Create())
    {
      byte[] data = new byte[4];
      rng.GetBytes(data);
      uint randomInt = BitConverter.ToUInt32(data, 0);
      return randomInt.ToString("X8");
    }
  }
}