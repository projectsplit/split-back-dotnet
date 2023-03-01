namespace SplitBackApi.Api.Extensions;

public static class StreamExtensions {

  public static async Task<string> ToSerializedString(this Stream stream) {

    var buffer = new MemoryStream();
    await stream.CopyToAsync(buffer);
    buffer.Seek(0, SeekOrigin.Begin);

    var serializedRequest = await new StreamReader(buffer).ReadToEndAsync();

    buffer.Seek(0, SeekOrigin.Begin);
    stream = buffer;

    return serializedRequest;
  }
}