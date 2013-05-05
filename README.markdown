# t2d-damage

Damage behaviors for the [Torque 2D][] game engine.

  [Torque 2D]: https://github.com/GarageGames/Torque2D

# Installation

I recommend cloning this repository right into your T2D folder:

```
cd modules/
git clone git@github.com:eightyeight/t2d-damage Damage
```

Or you can achieve the same effect by [downloading a ZIP file][Download].
Note that this will download the latest version of the `master` branch, which will be stable
but may be out of date.

  [Download]: https://github.com/eightyeight/t2d-damage/archive/master.zip

# Use

This module defines a number of behaviors that allow you to create damaging interactions.
These interactions are modeled as one-way scenarios with an 'attacker' and a 'defender',
but may have two-way side-effects like damage and physical movement.

## Damageable objects

To make an object damageable, call the `able` function on it:

    Damage.able(%obj, true);

The second argument is a Boolean that defaults to `true` and determines whether or not the object is damageable.
This gives the object a `damage` property that increases when it takes damage.

## Basic damage

Raw damage can be applied using several functions:

    Damage.just(%obj, %damage);
    Damage.everythingNear(%obj, %radius, %damage);
    Damage.everythingLeftOf(%obj, %distance, %damage);
    Damage.everythingRightOf(%obj, %distance, %damage);
    Damage.everythingAbove(%obj, %distance, %damage);
    Damage.everythingBelow(%obj, %distance, %damage);

Except the first method, no damage is applied to `%obj`.
In the very simplest case, `%damage` can be a number, the amount of damage to add to everything.

However, damage can also be in the form of a string where each word is a damage option.
For example,

    Damage.just(%obj, "amount:10 type:fire impulseX:50");

specifies the `fire` damage type and a horizontal impulse.
Note that this module does not define any damage types;
these are just strings that you can make use of in your own scripts as you deem appropriate.

## Destruction



## Attackers and defenders

However, sometimes the amount of damage is a complex function of equipment, levels, abilities and the damage scenario.
This module lets you add all sorts of optional effects to your objects to give attacks (and defence) different properties.
These effects all model attacks as originating from an _attacker_ and being applied to a _defender_.
In some cases this is literally true, such as two characters fighting.
In other cases, you may have a more abstract attacker, such as a spike trap, or a screen region effect.

You can initiate an _attack_ using the appropriate method:

    Damage.attack(%attacker, %defender, %damage);

Again, `%damage` can be a number or an option string.
There are also some convenience methods that allow you deal damage in common patterns to one or multiple defenders.
All the `everything` methods can be used with an `attack` in the middle, like:

    Damage.everythingAttackedNear(%attacker, %range, %damage);

Some common patterns with direction:

    Damage.attackFirstOnLine(%attacker, %direction, %length, %damage);
    Damage.attackAllOnLine(%attacker, %direction, %length, %damage);
    Damage.attackAllInArc(%attacker, %direction, %angle, %range, %damage);

In all these cases, `%direction` can be an angle in degrees from the positive _x_ axis, or a vector.

## Effects

Now, the exciting part of this attacker and defender stuff is that either party may have _effects_ applied to them.
For example, you might pick up a golden lollipop that makes all your attacks do twice the damage.
This is implemented as an attack multiplier effect.

To add attack and defence effects, use the `effect` method:

    %e = Damage.createAttackEffect();
    %e.multiplier = 2;
    %e.duration = 10;
    Damage.effect(%obj, %e);

For the next 10 seconds, this object's attacks will deal twice the damage.
