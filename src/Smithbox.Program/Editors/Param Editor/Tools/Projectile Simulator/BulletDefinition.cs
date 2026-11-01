using Andre.Formats;
using System;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
///     When a bullet may hand over to its chained bullets. The values match launchConditionType.
/// </summary>
public enum BulletChainCondition
{
    Always = 0,
    Water = 1,
    WaterOrSwamp = 2,
    NotCharacter = 3,
    CharacterOnly = 4,

    // Used throughout Elden Ring, for timed follow-ups as much as anything, but not defined by any TDF
    Undocumented = 5,

    // Only when another bullet is absorbed, which isn't something that happens in the simulation
    Absorbed = 6,

    NoImpact = 254,
    ImpactOnly = 255
}

/// <summary>
///     The direction that bullets created by a bullet are aimed relative to. The values match sfxPostureType.
/// </summary>
public enum BulletChildPosture
{
    Parent = 0,
    Horizontal = 1,
    Impact = 2
}

/// <summary>
///     Where a bullet starts. The values match EmittePosType.
/// </summary>
public enum BulletEmitPosition
{
    Source = 0,
    AroundSource = 1,
    AboveSource = 2,
    Target = 3,
    AroundTarget = 4,
    Parent = 5,
    BehindTarget = 6
}

/// <summary>
///     How a spell slot gets used. The values match consumeType.
/// </summary>
public enum SpellSlotType
{
    Default = 0,
    Charged = 1,
    None = 2
}

/// <summary>
///     A bullet in a spell row.
/// </summary>
public readonly record struct SpellBullet(int Id, SpellSlotType Slot);

/// <summary>
///     The parts of a bullet that have no effect on the simulation.
/// </summary>
[Flags]
public enum BulletIgnoredBehavior
{
    None = 0,
    Homing = 1,
    EmitPosition = 2,
    Following = 4,
    Arc = 8
}

/// <summary>
///     A bullet row with the differences between games taken out of it.
/// </summary>
public sealed class BulletDefinition
{
    private const float DefaultRadius = 0.2f;

    private const int BulletCategory = 1;

    public int RowId { get; init; }
    public string Name { get; init; }

    public AttackDefinition Attack { get; init; }

    // How long a hit is remembered for, after which the same target can be hit again. Forever at 0
    public float DamageInterval { get; init; }

    public float Life { get; init; }
    public float LifeRandom { get; init; }

    // Gravity and acceleration each have a value for before and after this distance has been covered
    public float Range { get; init; }
    public float GravityInRange { get; init; }
    public float GravityOutRange { get; init; }
    public float GravityStartTime { get; init; }
    public float AccelInRange { get; init; }
    public float AccelOutRange { get; init; }
    public float AccelStartTime { get; init; }
    public float AccelEndTime { get; init; } = float.MaxValue;
    public float InitialSpeed { get; init; }
    public float MaxSpeed { get; init; }
    public float MinSpeed { get; init; }
    public float ExternalForce { get; init; }

    public float Radius { get; init; }
    public float RadiusMax { get; init; }
    public float SpreadTime { get; init; }

    // The most the bullet can turn each second, in degrees
    public float HomingRateY { get; init; }
    public float HomingRateX { get; init; }
    public float HomingBeginDistance { get; init; }
    public float HomingBeginTime { get; init; }

    // Homing stops for good on any of these, which are off at 0
    public float HomingEndDistance { get; init; }
    public float HomingStopRange { get; init; }
    public float HomingStopAngle { get; init; }

    // Where the bullet aims relative to the target
    public float HomingOffset { get; init; }
    public float HomingHeightOffset { get; init; }

    // Volleys are fired one after the other, and each one is made up of shots fired at the same time
    public int VolleyCount { get; init; } = 1;
    public float VolleyInterval { get; init; }
    public float VolleyAngleY { get; init; }
    public float VolleyAngleX { get; init; }
    public int ShotsPerVolley { get; init; } = 1;
    public float ShotAngleY { get; init; }
    public float ShotAngleX { get; init; }

    public float AngleY { get; init; }
    public float AngleX { get; init; }
    public float AngleYRandom { get; init; }
    public float AngleXRandom { get; init; }

    public IReadOnlyList<int> ChainChildren { get; init; } = Array.Empty<int>();
    public BulletChainCondition ChainCondition { get; init; }
    public float ChainDelay { get; init; }

    public IReadOnlyList<int> IntervalChildren { get; init; } = Array.Empty<int>();
    public float IntervalMin { get; init; }
    public float IntervalMax { get; init; }
    public float IntervalWait { get; init; }
    public int IntervalLimit { get; init; }

    public bool FiresBackward { get; init; }

    // Every row that fires backward also has these, which lob it at the target when there is one
    public float HowitzerAngleMin { get; init; }
    public float HowitzerAngleMax { get; init; }
    public float HowitzerSpeedMin { get; init; }
    public float HowitzerSpeedMax { get; init; }

    public BulletEmitPosition EmitPosition { get; init; }

    // How far around its source or target a bullet can start. Older games have no field for this,
    // as they use a fixed distance
    public float? EmitRadius { get; init; }

    public BulletChildPosture ChildPosture { get; init; }
    public bool ChildAimedFromOwner { get; init; }
    public bool ChildInheritsSpeed { get; init; }

    public bool PenetratesMap { get; init; }
    public bool PenetratesCharacter { get; init; }

    public BulletIgnoredBehavior Ignored { get; init; }

    /// <summary>
    ///     False for the params whose layouts aren't known yet, where the fields only have placeholder names.
    /// </summary>
    public static bool IsReadable(Param.Row row)
    {
        return row["initVellocity"] != null || row["initialVelocity"] != null;
    }

    /// <param name="row">The bullet row.</param>
    /// <param name="findAttack">Finds an attack by its row ID, returning null if there is no such row.</param>
    public static BulletDefinition FromRow(Param.Row row, Func<int, AttackDefinition> findAttack)
    {
        return row["initialVelocity"] != null ? FromDS2Row(row) : FromStandardRow(row, findAttack);
    }

    /// <summary>
    ///     The bullets in a spell row, which has slots that can also hold attacks and special effects.
    /// </summary>
    public static SpellBullet[] GetSpellBullets(Param.Row row)
    {
        List<SpellBullet> bullets = new();

        AddSpellBullet(bullets, row, "refCategory", "refId", "consumeType");

        for (var slot = 1; slot <= 10; slot++)
        {
            AddSpellBullet(bullets, row, $"refCategory{slot}", $"refId{slot}", $"consumeType{slot}");
            AddSpellBullet(bullets, row, $"refCategory_slot{slot}", $"refId_slot{slot}", $"consumeType_slot{slot}");
        }

        if (Int(row, 0, "baseSpellBulletId") is > 0 and var bullet)
        {
            bullets.Add(new SpellBullet(bullet, SpellSlotType.Default));
        }

        return bullets.DistinctBy(entry => entry.Id).ToArray();
    }

    private static void AddSpellBullet(List<SpellBullet> bullets, Param.Row row, string category, string id, string slot)
    {
        if (Int(row, -1, category) == BulletCategory && Int(row, 0, id) is > 0 and var bullet)
        {
            bullets.Add(new SpellBullet(bullet, (SpellSlotType)Int(row, 0, slot)));
        }
    }

    private static BulletDefinition FromStandardRow(Param.Row row, Func<int, AttackDefinition> findAttack)
    {
        var radius = Float(row, 0, "hitRadius");
        var radiusMax = Float(row, -1, "hitRadiusMax");
        var homingAngle = Float(row, 0, "homingAngle");
        var homingAngleX = Float(row, -1, "homingAngleX", "homingCorrectAngle");

        var ignored = BulletIgnoredBehavior.None;

        if (homingAngle > 0)
        {
            ignored |= BulletIgnoredBehavior.Homing;
        }

        var emitPosition = (BulletEmitPosition)Int(row, 0, "EmittePosType");

        if (emitPosition is BulletEmitPosition.Target or BulletEmitPosition.AroundTarget or BulletEmitPosition.BehindTarget)
        {
            ignored |= BulletIgnoredBehavior.EmitPosition;
        }

        if (Int(row, 0, "FollowType") != 0)
        {
            ignored |= BulletIgnoredBehavior.Following;
        }

        if (Int(row, 0, "ballisticCalcType") == 1)
        {
            ignored |= BulletIgnoredBehavior.Arc;
        }

        var attack = Int(row, -1, "atkId_Bullet", "atkBullet_Id");

        return new BulletDefinition
        {
            RowId = row.ID,
            Name = row.Name,

            Attack = attack > 0 ? findAttack(attack) : null,
            DamageInterval = Float(row, 0, "dmgHitRecordLifeTime"),

            Life = Float(row, -1, "life"),
            LifeRandom = Float(row, 0, "lifeRandomRange"),

            Range = Float(row, 0, "dist"),
            GravityInRange = Float(row, 0, "gravityInRange"),
            GravityOutRange = Float(row, 0, "gravityOutRange"),
            AccelInRange = Float(row, 0, "accelInRange"),
            AccelOutRange = Float(row, 0, "accelOutRange"),
            AccelStartTime = Float(row, 0, "accelTime"),
            AccelEndTime = Float(row, float.MaxValue, "accelEndTime"),
            InitialSpeed = Float(row, 0, "initVellocity"),
            MaxSpeed = Float(row, 0, "maxVellocity"),
            MinSpeed = Float(row, 0, "minVellocity"),
            ExternalForce = Float(row, 0, "externalForce"),

            Radius = radius,
            RadiusMax = radiusMax < 0 ? radius : radiusMax,
            SpreadTime = Float(row, 0, "spreadTime"),

            HomingRateY = homingAngle,
            HomingRateX = homingAngleX < 0 ? homingAngle : homingAngleX,
            HomingBeginDistance = Float(row, 0, "homingBeginDist"),
            HomingBeginTime = Float(row, 0, "homingBeginTime"),
            HomingStopRange = Float(row, 0, "hormingStopRange", "homingStopRange"),
            HomingOffset = Float(row, 0, "hormingOffsetRange", "homingOffsetRange"),
            HomingHeightOffset = Float(row, 0, "targetYOffsetRange"),

            VolleyCount = Math.Max(1, Int(row, 1, "numShoot")),
            VolleyInterval = Float(row, 0, "shootInterval"),
            VolleyAngleY = Float(row, 0, "shootAngleInterval"),
            VolleyAngleX = Float(row, 0, "shootAngleXInterval"),

            AngleY = Float(row, 0, "shootAngle"),
            AngleX = Float(row, 0, "shootAngleXZ"),
            AngleYRandom = Float(row, 0, "shootAngleYMaxRandom"),
            AngleXRandom = Float(row, 0, "shootAngleXMaxRandom"),

            ChainChildren = Ids(row, "HitBulletID", "hitBulletId"),
            ChainCondition = (BulletChainCondition)Int(row, 0, "launchConditionType"),
            ChainDelay = Float(row, 0, "expDelay"),

            IntervalChildren = Ids(row, "intervalCreateBulletId"),
            IntervalMin = Float(row, 0, "intervalCreateTimeMin"),
            IntervalMax = Float(row, 0, "intervalCreateTimeMax"),
            IntervalWait = Float(row, 0, "intervalCreateWaitTime"),
            IntervalLimit = Int(row, 0, "intervalCreateBulletMax"),

            FiresBackward = Int(row, 0, "ballisticCalcType") == 1,
            HowitzerAngleMin = Float(row, 0, "howitzerShootAngleXMin"),
            HowitzerAngleMax = Float(row, 0, "howitzerShootAngleXMax"),
            HowitzerSpeedMin = Float(row, 0, "howitzerInitMinVelocity"),
            HowitzerSpeedMax = Float(row, 0, "howitzerInitMaxVelocity"),
            EmitPosition = emitPosition,
            EmitRadius = row["randomCreateRadius"] != null ? Float(row, 0, "randomCreateRadius") : null,

            // Games without this field measure from the horizontal, which is how shootAngleXZ is described
            ChildPosture = (BulletChildPosture)Int(row, (int)BulletChildPosture.Horizontal, "sfxPostureType"),
            ChildAimedFromOwner = Int(row, 0, "isOwnerOverrideInitAngle") != 0,
            ChildInheritsSpeed = Int(row, 0, "isInheritSpeedToChild", "IsInheritSpeedToChild") != 0,

            PenetratesMap = Int(row, 0, "isPenetrateMap", "IsPenetrateMap") != 0,
            PenetratesCharacter = Int(row, 0, "isPenetrateChr", "isPenetrate", "charaPenetrateType") != 0,

            Ignored = ignored
        };
    }

    private static BulletDefinition FromDS2Row(Param.Row row)
    {
        var chainOnImpact = Int(row, 0, "isChildeExplosion") != 0;
        var chainOnExpiry = Int(row, 0, "isChildeLife") != 0;
        var interval = Int(row, 0, "isChildeContinue") != 0;
        var intervalTime = Float(row, 0, "childeInterval");

        var condition = chainOnImpact && chainOnExpiry ? BulletChainCondition.Always
            : chainOnImpact ? BulletChainCondition.ImpactOnly
            : BulletChainCondition.NoImpact;

        var children = Ids(row, "childeBulletId", "childeBulletId02", "childeBulletId03");
        var homingAngle = Angle(row, "homingAngle");

        return new BulletDefinition
        {
            RowId = row.ID,
            Name = row.Name,

            Life = Float(row, -1, "bulletLife"),

            GravityInRange = Float(row, 0, "gravityValue"),
            GravityOutRange = Float(row, 0, "gravityValue"),
            GravityStartTime = Float(row, 0, "downStartTime"),
            AccelInRange = Float(row, 0, "acceleration"),
            AccelOutRange = Float(row, 0, "acceleration"),
            AccelStartTime = Float(row, 0, "acceleStartTime"),
            InitialSpeed = Float(row, 0, "initialVelocity"),
            MaxSpeed = Float(row, 0, "maxSpeed"),
            ExternalForce = Float(row, 0, "externalForce"),

            // The size of the hit is set by the attack, rather than here
            Radius = DefaultRadius,
            RadiusMax = DefaultRadius,

            HomingRateY = homingAngle,
            HomingRateX = homingAngle,
            HomingBeginTime = Float(row, 0, "homingStartTime"),
            HomingEndDistance = Float(row, 0, "homingStopDistance"),
            HomingStopRange = Float(row, 0, "homingStopTargetDistance"),
            HomingStopAngle = Angle(row, "homingStopAngle"),

            VolleyCount = Math.Max(1, Int(row, 1, "automaticShootNum")),
            VolleyInterval = Float(row, 0, "automaticInterval"),
            VolleyAngleY = Angle(row, "automaticAngleY"),
            VolleyAngleX = Angle(row, "automaticAngleXZ"),
            ShotsPerVolley = Math.Max(1, Int(row, 1, "simultaneouslyNum")),
            ShotAngleY = Angle(row, "simultaneouslyAngleY"),
            ShotAngleX = Angle(row, "simultaneouslyAngleXZ"),

            AngleY = Angle(row, "shootAngle"),
            AngleX = Angle(row, "shootAngleXZ"),

            ChainChildren = chainOnImpact || chainOnExpiry ? children : Array.Empty<int>(),
            ChainCondition = condition,
            ChainDelay = Float(row, 0, "childeDelay"),

            IntervalChildren = interval ? children : Array.Empty<int>(),
            IntervalMin = intervalTime,
            IntervalMax = intervalTime,

            PenetratesMap = Int(row, 0, "penetrationMap") != 0,
            PenetratesCharacter = Int(row, 0, "penetration") != 0,

            Ignored = homingAngle > 0 ? BulletIgnoredBehavior.Homing : BulletIgnoredBehavior.None
        };
    }

    private static float Float(Param.Row row, float fallback, params string[] names)
    {
        foreach (var name in names)
        {
            if (row[name] is { } cell)
            {
                return Convert.ToSingle(cell.Value);
            }
        }

        return fallback;
    }

    // Angles are in degrees everywhere else, but angle32 fields hold radians
    private static float Angle(Param.Row row, string name)
    {
        return float.RadiansToDegrees(Float(row, 0, name));
    }

    private static int Int(Param.Row row, int fallback, params string[] names)
    {
        foreach (var name in names)
        {
            if (row[name] is { } cell)
            {
                return (int)Convert.ToInt64(cell.Value);
            }
        }

        return fallback;
    }

    private static int[] Ids(Param.Row row, params string[] names)
    {
        return names.Select(name => Int(row, 0, name)).Where(id => id > 0).Distinct().ToArray();
    }
}
