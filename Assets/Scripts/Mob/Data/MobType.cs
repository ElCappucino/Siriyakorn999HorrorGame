namespace MobSystem
{
    /// <summary>
    /// Enum defining all available mob types
    /// </summary>
    public enum MobType
    {
        Sadako,              // Slow crawling ghost with electricity
        Kumarn,              // Fast running child ghost with Thai voice
        Vampire,             // Normal speed bat that transforms to humanoid jumpscare
        SmolSadako,          // Faster crawling mini-Sadako with electricity + child voice
        KumarnBatWing,       // Fast flying/hovering with pause before attack + child voice + bat
        VampireLongHair      // Normal speed bat that transforms immediately + electricity + bat
    }

    /// <summary>
    /// Movement type for mobs
    /// </summary>
    public enum MovementType
    {
        Walking,
        Crawling,
        Flying,
        Hovering
    }

    /// <summary>
    /// Speed tier for mobs
    /// </summary>
    public enum SpeedTier
    {
        Slow,       // 1.5-2.5
        Normal,     // 3.0-4.0
        Fast        // 5.0-7.0
    }

    /// <summary>
    /// Audio theme for mobs
    /// </summary>
    public enum AudioTheme
    {
        Electricity,        // แฟ่ไฟฟ้า
        ChildVoice,        // แฟยินไทย / ฮันไทย
        Bat,               // แฟไม่กาเชน (bat/vampire sounds)
        ElectricityChild,  // Combined electricity + child voice
        ChildBat          // Combined child voice + bat sounds
    }
}

