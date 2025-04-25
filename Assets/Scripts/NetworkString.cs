using Unity.Netcode;
using Unity.Collections;
using System;


// Thank you ChatGPT I am not relearning EECS 280 for this
// Network Variables need to use NetworkString
// However, it can be converted to a string implicitly
[System.Serializable]
public struct NetworkString : INetworkSerializable, IEquatable<NetworkString> {
    private FixedString64Bytes value;

    public string Value {
        get => value.ToString();
        set => this.value = new FixedString64Bytes(value);
    }

    public NetworkString(string value) {
        this.value = new FixedString64Bytes(value);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref value);
    }

    public bool Equals(NetworkString other) {
        return value.Equals(other.value);
    }

    public override bool Equals(object obj) {
        if (obj is NetworkString other) {
            return Equals(other);
        }
        return false;
    }

    public override int GetHashCode() {
        return value.GetHashCode();
    }

    public static bool operator ==(NetworkString left, NetworkString right) {
        return left.Equals(right);
    }

    public static bool operator !=(NetworkString left, NetworkString right) {
        return !(left == right);
    }

    // Implicit conversion from string to NetworkString
    public static implicit operator NetworkString(string value) {
        return new NetworkString(value);
    }

    // Implicit conversion from NetworkString to string
    public static implicit operator string(NetworkString networkString) {
        return networkString.Value;
    }

    public override string ToString() {
        return Value.ToString();
    }
}
