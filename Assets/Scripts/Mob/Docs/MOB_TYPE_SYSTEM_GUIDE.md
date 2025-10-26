# Mob Type System Guide

## Overview
The mob system has been refactored to support multiple mob types with unique behaviors. Each mob type can have its own movement patterns, sounds, special abilities, and visual effects.

## Available Mob Types

### 1. **Sadako**
- **Speed:** Slow
- **Movement:** Crawling
- **Audio Theme:** Electricity (Computer Noise)
- **Special:** Periodic electricity effects while chasing

### 2. **Kumarn**
- **Speed:** Fast
- **Movement:** Running with zigzag pattern
- **Audio Theme:** Thai Child Voice (Bell Anklet)
- **Special:** Random screaming, erratic movement patterns

### 3. **Vampire**
- **Speed:** Normal
- **Movement:** Flies as bat, then walks as humanoid
- **Audio Theme:** Bat sounds (Window Open-Close)
- **Special:** Transforms from bat to humanoid when close to player, creates jumpscare

### 4. **Smol Sadako**
- **Speed:** Normal (faster than regular Sadako)
- **Movement:** Crawling
- **Audio Theme:** Electricity + Child Voice
- **Special:** Combines electricity effects with child giggles

### 5. **Kumarn Bat Wing**
- **Speed:** Fast
- **Movement:** Flying/Hovering
- **Audio Theme:** Child Voice + Bat
- **Special:** Pauses before attacking, hovering movement

### 6. **Vampire Long Hair**
- **Speed:** Normal
- **Movement:** Brief bat flight, then humanoid
- **Audio Theme:** Electricity + Bat
- **Special:** Transforms immediately without needing to be close to player

## Architecture

### Core Components

#### 1. **MobData (ScriptableObject)**
Defines the characteristics of a mob type:
- Movement type (Walking, Crawling, Flying, Hovering)
- Speed tier (Slow, Normal, Fast)
- Detection and attack ranges
- Health and damage values
- Audio theme and sound names
- Special behavior flags (transformation, pause, jumpscare)

**Location:** Create via `Assets > Create > Horror Game > Mob Data`

#### 2. **MobBehavior (Abstract Base Class)**
Base class for all mob-specific behaviors. Provides hooks for:
- `OnSpawned()` - Called when mob spawns
- `OnIdleUpdate()` - Called each frame while idle
- `OnStartChasing()` - Called when starting to chase
- `OnChasingUpdate()` - Called each frame while chasing
- `OnStartAttacking()` - Called when starting attack
- `OnAttackingUpdate()` - Called each frame while attacking
- `OnAttack()` - Called when performing attack
- `OnTakeDamage()` - Called when taking damage
- `OnDeath()` - Called when dying
- `CustomMovement()` - Override for custom movement
- `GetSpeedMultiplier()` - Dynamic speed modification

#### 3. **MobAI**
Main AI controller that:
- Uses MobData to configure mob
- Calls appropriate behavior hooks
- Handles state machine (Idle, Chasing, Attacking)
- Manages NavMesh movement
- Supports custom destinations and pause states

#### 4. **Specific Behavior Scripts**
Each mob type has its own behavior script:
- `SadakoBehavior` - Electricity effects
- `KumarnBehavior` - Screaming and zigzag movement
- `VampireBehavior` - Bat transformation
- `SmolSadakoBehavior` - Electricity + giggles
- `KumarnBatWingBehavior` - Hovering with pause
- `VampireLongHairBehavior` - Immediate transformation

## How to Create a New Mob

### Step 1: Create MobData Asset
1. Right-click in Project window
2. Select `Create > Horror Game > Mob Data`
3. Configure the settings:
   - Set mob type
   - Configure movement and speed
   - Set detection/attack ranges
   - Configure audio settings
   - Enable special behaviors if needed

### Step 2: Create or Choose a Prefab
1. Create a new GameObject or use existing mob prefab
2. Add these components:
   - **MobAI** - Main AI controller
   - **MobHealth** - Health and damage handling
   - **MobAnimationController** - Animation control
   - **NavMeshAgent** - For movement
   - **Specific Behavior Script** (e.g., SadakoBehavior)

### Step 3: Configure MobAI
1. Assign the MobData you created
2. The AI will automatically apply settings from MobData
3. You can still override settings in the inspector if needed

### Step 4: Configure Behavior Script
1. Add the appropriate behavior script (must match mob type)
2. Configure behavior-specific settings (effects, intervals, etc.)

### Step 5: Setup Spawner
1. Create a GameObject with **MobSpawner** component
2. Assign the mob prefab
3. Optionally assign MobData override to spawn different variants
4. Configure spawn type (Object, Ceiling, Wall, Floor)
5. Set trigger type (Proximity, Timed, Manual)

## Creating a Custom Mob Behavior

```csharp
using UnityEngine;

namespace MobSystem
{
    public class MyCustomBehavior : MobBehavior
    {
        [Header("Custom Settings")]
        [SerializeField] private float customValue = 1f;

        public override void OnSpawned()
        {
            // Called when mob spawns
            Debug.Log("Custom mob spawned!");
        }

        public override void OnChasingUpdate(float distanceToPlayer)
        {
            // Called every frame while chasing
            // Add custom logic here
        }

        public override bool CustomMovement()
        {
            // Return true to use custom movement instead of NavMesh
            // Use mobAI.SetCustomDestination() to set where to move
            return false; // Use default NavMesh movement
        }

        public override float GetSpeedMultiplier()
        {
            // Return speed multiplier (1.0 = normal speed)
            return 1.5f; // 50% faster
        }
    }
}
```

## Audio Integration

The system uses **MobAudioManager** for 3D spatial audio:
- Detection sounds play when mob spots player
- Chase sounds can loop while pursuing
- Attack sounds play during attacks
- Custom sounds can be triggered in behaviors

Example:
```csharp
MobAudioManager.instance.PlayAudio3D("SoundName", transform.position);
```

## Best Practices

1. **Always test with NavMesh** - Ensure your scene has a NavMesh baked
2. **Use MobData for variants** - Create different MobData assets for mob variations
3. **Keep behaviors focused** - Each behavior should handle one mob type's unique logic
4. **Audio naming** - Use consistent naming for audio clips (e.g., "Sadako_Spawn", "Kumarn_Scream")
5. **Performance** - For flying mobs, consider disabling NavMesh and using custom movement
6. **Visual effects** - Reference effects in behavior scripts, not in base AI

## Debugging

- Enable "Show Debug Gizmos" on MobAI to see detection ranges
- Check console for spawn messages
- Use MobAI.GetCurrentState() to check mob state
- Behaviors can override OnDrawGizmosSelected() for visual debugging

## Common Issues

**Mob not moving:**
- Check if NavMesh is baked
- Ensure NavMeshAgent is properly configured
- Check if mob is paused (SetPaused)

**Behavior not working:**
- Ensure behavior script is attached to same GameObject as MobAI
- Check if MobData is assigned
- Verify behavior class name matches in MobData

**Audio not playing:**
- Ensure MobAudioManager exists in scene
- Check audio clip names match
- Verify 3D audio settings

## Future Extensions

To add a new mob type:
1. Add new enum value to `MobType` enum
2. Create new MobData asset with the type
3. Create new behavior script inheriting from `MobBehavior`
4. Implement unique behavior in the new script
5. Create prefab with all required components
6. Test in scene with spawner

---

**Created:** October 2025
**Version:** 1.0
**For:** Siriyakorn999 Horror Game

