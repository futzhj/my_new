using System;

namespace Spine3_8_95;

[Flags]
public enum TransformMode
{
	Normal = 0,
	OnlyTranslation = 7,
	NoRotationOrReflection = 1,
	NoScale = 2,
	NoScaleOrReflection = 6
}
