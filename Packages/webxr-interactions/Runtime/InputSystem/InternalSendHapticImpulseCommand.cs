#if UNITY_INPUT_SYSTEM_1_4_4_OR_NEWER
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace WebXR.InputSystem
{
  [StructLayout(LayoutKind.Explicit, Size = kSize)]
  internal struct InternalSendHapticImpulseCommand
  {
    internal static FourCC Type => new FourCC('X', 'H', 'I', '0');

    private const int kSize = InputDeviceCommand.BaseCommandSize + sizeof(int) + (sizeof(float) * 2);

    [FieldOffset(0)]
    internal InputDeviceCommand baseCommand;

    [FieldOffset(InputDeviceCommand.BaseCommandSize)]
    internal int channel;

    [FieldOffset(InputDeviceCommand.BaseCommandSize + sizeof(int))]
    internal float amplitude;

    [FieldOffset(InputDeviceCommand.BaseCommandSize + sizeof(int) + (sizeof(float)))]
    internal float duration;
  }
}
#endif
