using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.ComponentModel;

namespace ZuneModdingHelper.Services
{
    /// <summary>
    /// An <see cref="IAsyncSerializer{TSerialized}"/> and implementation for serializing and deserializing streams using System.Text.Json.
    /// </summary>
    public class SystemTextJsonStreamSerializer : IAsyncSerializer<Stream>, ISerializer<Stream>
    {
        /// <summary>
        /// A singleton instance for <see cref="SystemTextJsonStreamSerializer"/>.
        /// </summary>
        public static SystemTextJsonStreamSerializer Singleton { get; } = new();

        /// <inheritdoc />
        public async Task<Stream> SerializeAsync<T>(T data, CancellationToken? cancellationToken = null)
        {
            MemoryStream stream = new();
            await JsonSerializer.SerializeAsync(stream, data, cancellationToken: cancellationToken.GetValueOrDefault());
            return stream;
        }

        /// <inheritdoc />
        public async Task<Stream> SerializeAsync(Type inputType, object data, CancellationToken? cancellationToken = null)
        {
            MemoryStream stream = new();
            await JsonSerializer.SerializeAsync(stream, data, inputType, cancellationToken: cancellationToken.GetValueOrDefault());
            return stream;
        }

        /// <inheritdoc />
        public async Task<TResult> DeserializeAsync<TResult>(Stream serialized, CancellationToken? cancellationToken = null)
        {
            return await JsonSerializer.DeserializeAsync<TResult>(serialized, cancellationToken: cancellationToken.GetValueOrDefault())
                ?? default;
        }

        /// <inheritdoc />
        public async Task<object> DeserializeAsync(Type returnType, Stream serialized, CancellationToken? cancellationToken = null)
        {
            return await JsonSerializer.DeserializeAsync(serialized, returnType, cancellationToken: cancellationToken.GetValueOrDefault())
                ?? default;
        }

        /// <inheritdoc />
        public Stream Serialize<T>(T data) => Serialize(typeof(T), data);

        /// <inheritdoc />
        public Stream Serialize(Type type, object data)
        {
            MemoryStream stream = new();
            JsonSerializer.Serialize(stream, data, type);
            return stream;
        }

        /// <inheritdoc />
        public TResult Deserialize<TResult>(Stream serialized) => JsonSerializer.Deserialize<TResult>(serialized);

        /// <inheritdoc />
        public object Deserialize(Type type, Stream serialized) => JsonSerializer.Deserialize(serialized, type);
    }
}