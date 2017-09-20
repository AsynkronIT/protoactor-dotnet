// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: Protos.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Proto.Cluster {

  /// <summary>Holder for reflection information generated from Protos.proto</summary>
  public static partial class ProtosReflection {

    #region Descriptor
    /// <summary>File descriptor for Protos.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ProtosReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgxQcm90b3MucHJvdG8SB2NsdXN0ZXIaGFByb3RvLkFjdG9yL3Byb3Rvcy5w",
            "cm90byI2Cg1UYWtlT3duZXJzaGlwEhcKA3BpZBgBIAEoCzIKLmFjdG9yLlBJ",
            "RBIMCgRuYW1lGAIgASgJIjoKDEdyYWluUmVxdWVzdBIUCgxtZXRob2RfaW5k",
            "ZXgYASABKAUSFAoMbWVzc2FnZV9kYXRhGAIgASgMIiUKDUdyYWluUmVzcG9u",
            "c2USFAoMbWVzc2FnZV9kYXRhGAEgASgMIiEKEkdyYWluRXJyb3JSZXNwb25z",
            "ZRILCgNlcnIYASABKAlCEKoCDVByb3RvLkNsdXN0ZXJiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Proto.ProtosReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Proto.Cluster.TakeOwnership), global::Proto.Cluster.TakeOwnership.Parser, new[]{ "Pid", "Name" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Proto.Cluster.GrainRequest), global::Proto.Cluster.GrainRequest.Parser, new[]{ "MethodIndex", "MessageData" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Proto.Cluster.GrainResponse), global::Proto.Cluster.GrainResponse.Parser, new[]{ "MessageData" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Proto.Cluster.GrainErrorResponse), global::Proto.Cluster.GrainErrorResponse.Parser, new[]{ "Err" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class TakeOwnership : pb::IMessage<TakeOwnership> {
    private static readonly pb::MessageParser<TakeOwnership> _parser = new pb::MessageParser<TakeOwnership>(() => new TakeOwnership());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TakeOwnership> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Proto.Cluster.ProtosReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TakeOwnership() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TakeOwnership(TakeOwnership other) : this() {
      Pid = other.pid_ != null ? other.Pid.Clone() : null;
      name_ = other.name_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TakeOwnership Clone() {
      return new TakeOwnership(this);
    }

    /// <summary>Field number for the "pid" field.</summary>
    public const int PidFieldNumber = 1;
    private global::Proto.PID pid_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Proto.PID Pid {
      get { return pid_; }
      set {
        pid_ = value;
      }
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 2;
    private string name_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TakeOwnership);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TakeOwnership other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Pid, other.Pid)) return false;
      if (Name != other.Name) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (pid_ != null) hash ^= Pid.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (pid_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Pid);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (pid_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Pid);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TakeOwnership other) {
      if (other == null) {
        return;
      }
      if (other.pid_ != null) {
        if (pid_ == null) {
          pid_ = new global::Proto.PID();
        }
        Pid.MergeFrom(other.Pid);
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (pid_ == null) {
              pid_ = new global::Proto.PID();
            }
            input.ReadMessage(pid_);
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
        }
      }
    }

  }

  public sealed partial class GrainRequest : pb::IMessage<GrainRequest> {
    private static readonly pb::MessageParser<GrainRequest> _parser = new pb::MessageParser<GrainRequest>(() => new GrainRequest());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<GrainRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Proto.Cluster.ProtosReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GrainRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GrainRequest(GrainRequest other) : this() {
      methodIndex_ = other.methodIndex_;
      messageData_ = other.messageData_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GrainRequest Clone() {
      return new GrainRequest(this);
    }

    /// <summary>Field number for the "method_index" field.</summary>
    public const int MethodIndexFieldNumber = 1;
    private int methodIndex_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int MethodIndex {
      get { return methodIndex_; }
      set {
        methodIndex_ = value;
      }
    }

    /// <summary>Field number for the "message_data" field.</summary>
    public const int MessageDataFieldNumber = 2;
    private pb::ByteString messageData_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString MessageData {
      get { return messageData_; }
      set {
        messageData_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as GrainRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(GrainRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (MethodIndex != other.MethodIndex) return false;
      if (MessageData != other.MessageData) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (MethodIndex != 0) hash ^= MethodIndex.GetHashCode();
      if (MessageData.Length != 0) hash ^= MessageData.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (MethodIndex != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(MethodIndex);
      }
      if (MessageData.Length != 0) {
        output.WriteRawTag(18);
        output.WriteBytes(MessageData);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (MethodIndex != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(MethodIndex);
      }
      if (MessageData.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(MessageData);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(GrainRequest other) {
      if (other == null) {
        return;
      }
      if (other.MethodIndex != 0) {
        MethodIndex = other.MethodIndex;
      }
      if (other.MessageData.Length != 0) {
        MessageData = other.MessageData;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            MethodIndex = input.ReadInt32();
            break;
          }
          case 18: {
            MessageData = input.ReadBytes();
            break;
          }
        }
      }
    }

  }

  public sealed partial class GrainResponse : pb::IMessage<GrainResponse> {
    private static readonly pb::MessageParser<GrainResponse> _parser = new pb::MessageParser<GrainResponse>(() => new GrainResponse());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<GrainResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Proto.Cluster.ProtosReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GrainResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GrainResponse(GrainResponse other) : this() {
      messageData_ = other.messageData_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GrainResponse Clone() {
      return new GrainResponse(this);
    }

    /// <summary>Field number for the "message_data" field.</summary>
    public const int MessageDataFieldNumber = 1;
    private pb::ByteString messageData_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString MessageData {
      get { return messageData_; }
      set {
        messageData_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as GrainResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(GrainResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (MessageData != other.MessageData) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (MessageData.Length != 0) hash ^= MessageData.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (MessageData.Length != 0) {
        output.WriteRawTag(10);
        output.WriteBytes(MessageData);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (MessageData.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(MessageData);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(GrainResponse other) {
      if (other == null) {
        return;
      }
      if (other.MessageData.Length != 0) {
        MessageData = other.MessageData;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            MessageData = input.ReadBytes();
            break;
          }
        }
      }
    }

  }

  public sealed partial class GrainErrorResponse : pb::IMessage<GrainErrorResponse> {
    private static readonly pb::MessageParser<GrainErrorResponse> _parser = new pb::MessageParser<GrainErrorResponse>(() => new GrainErrorResponse());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<GrainErrorResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Proto.Cluster.ProtosReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GrainErrorResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GrainErrorResponse(GrainErrorResponse other) : this() {
      err_ = other.err_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public GrainErrorResponse Clone() {
      return new GrainErrorResponse(this);
    }

    /// <summary>Field number for the "err" field.</summary>
    public const int ErrFieldNumber = 1;
    private string err_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Err {
      get { return err_; }
      set {
        err_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as GrainErrorResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(GrainErrorResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Err != other.Err) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Err.Length != 0) hash ^= Err.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Err.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Err);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Err.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Err);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(GrainErrorResponse other) {
      if (other == null) {
        return;
      }
      if (other.Err.Length != 0) {
        Err = other.Err;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            Err = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
