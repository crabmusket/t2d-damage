function Damage::create(%this) {
   if(!isObject(Damage.Effects)) {
      Damage.Effects = new SimSet();
   }
}

function Damage::destroy(%this) {
   if(isObject(Damage.Effects)) {
      Damage.Effects.deleteObjects();
      Damage.Effects.delete();
      Damage.Effects = "";
   }
}

function Damage::able(%this, %obj, %enable) {
   if(%enable $= "") {
      %enable = true;
   }

   if(%enable && !%obj.isDamageEnabled) {
      %obj.damage = 0;
      %obj.isDamageEnabled = true;
   }

   if(!%enable) {
      %obj.isDamageEnabled = false;
   }
}

function Damage::createAttackEffect(%this, %name) {
   %bt = new BehaviorTemplate(%name);
   %bt.damageEffectClass = "AttackEffectBehavior";
   Damage.Effects.add(%bt);
}

function Damage::createDefendEffect(%this, %name) {
   %bt = new BehaviorTemplate(%name);
   %bt.damageEffectClass = "DefendEffectBehavior";
   Damage.Effects.add(%bt);
}

function Damage::effect(%this, %obj, %effect) {
   %bi = %effect.createInstance();
   %bi.superClass = %effect.damageEffectClass;
   %obj.addBehavior(%bi);
}

function Damage::damage(%this, %attacker, %defender, %dam) {
   if(!%defender.isDamageEnabled) {
      return;
   }

   // Parameters that will be filled in later.
   %types = "";
   %amount = 0;
   %impulse = "0 0";
   %mul = 1;
   %extra = 0;

   if(getWordCount(%dam) == 1 && strchr(%dam, ":") == "") {
      %amount = %dam;
   } else {
      // Parse options.
   }

   // Get attack and defence behaviors.
   %attackB = "";
   for(%i = 0; %i < %attacker.getBehaviorCount(); %i++) {
      if(%attacker.getBehaviorByIndex(%i).superClass $= "AttackBehaviorType") {
         %attackB = %attackB SPC %attacker.getBehaviorByIndex(%i);
      }
   }
   %defendB = "";
   for(%i = 0; %i < %defender.getBehaviorCount(); %i++) {
      if(%defender.getBehaviorByIndex(%i).superClass $= "DefendBehaviorType") {
         %defendB = %defendB SPC %defender.getBehaviorByIndex(%i);
      }
   }
   %bs = %attackB SPC %defendB;

   // Get damage types from the behaviors.
   foreach$(%b in %bs) { %types = %types SPC %b.getDamageType(%attacker, %defender); }
   // Get damage modifiers and extra given the type.
   foreach$(%b in %bs) { %mul *= %b.getDamageMod(%attacker, %defender, %types); %extra += %b.getExtraDamage(%attacker, %defender, %types); }

   // Apply damage multiplier and extra damage.
   %amount = %amount * %mul + %extra;

   // Get impulse.
   foreach$(%b in %bs) { %impulse = vectorAdd(%impulse, %b.getImpulse(%attacker, %defender, %types, %amount)); }

   // Apply the damage.
   %defender.damage += %amount;
   // Apply the impulse.
   %defender.applyImpulse(%impulse, %defender.getPosition());

   // Let everyone know about the results.
   foreach$(%b in %bs) { %b.onDamage(%attacker, %defender, %types, %amount, %impulse); }
   // Let the object do custom stuff.
   %defender.onDamage(%attacker, %types, %amount, %impulse);
}

function DamageEffectBehavior::onBehaviorAdd(%this) {
   %this.startTime = getSimTime();
}

function DamageEffectBehavior::onUpdate(%this) {
   if(%this.duration $= "") {
      return;
   }

   // Check duration and end behavior if appropriate.
   if(getSimTime() - %this.startTime > (%this.duration * 1000)) {
      %this.owner.schedule(0, removeBehavior, %this);
      %this.schedule(1000, delete);
   }
}

function DamageEffectBehavior::getDamageType(%this, %attacker, %defender) {
   return "";
}

function DamageEffectBehavior::getDamageMod(%this, %attacker, %defender, %types) {
   return 1;
}

function DamageEffectBehavior::getExtraDamage(%this, %attacker, %defender, %types) {
   return 0;
}

function DamageEffectBehavior::getImpulse(%this, %attacker, %defender, %types, %amount) {
   return "0 0";
}

function DamageEffectBehavior::onDamage(%this, %attacker, %defender, %types, %amount, %impulse) {
}

// Ohmygosh.
function AttackEffectBehavior::onBehaviorAdd(%this) { DamageEffectBehavior::onBehaviorAdd(%this); }
function AttackEffectBehavior::onUpdate(%this) { DamageEffectBehavior::onUpdate(%this); }
function AttackEffectBehavior::getDamageType(%this, %a, %d) { return DamageEffectBehavior::getDamageType(%this, %a, %d); }
function AttackEffectBehavior::getDamageMod(%this, %a, %d, %t) { return DamageEffectBehavior::getDamageMod(%this, %a, %d, %t); }
function AttackEffectBehavior::getExtraDamage(%this, %a, %d, %t) { return DamageEffectBehavior::getExtraDamage(%this, %a, %d, %t); }
function AttackEffectBehavior::getImpulse(%this, %a, %d, %t, %amnt) { return DamageEffectBehavior::getImpulse(%this, %a, %d, %t, %amnt); }
function AttackEffectBehavior::onDamage(%this, %a, %d, %t, %amnt, %imp) { return DamageEffectBehavior::onDamage(%this, %a, %d, %t, %amnt, %imp); }
function DefendEffectBehavior::onBehaviorAdd(%this) { DamageEffectBehavior::onBehaviorAdd(%this); }
function DefendEffectBehavior::onUpdate(%this) { DamageEffectBehavior::onUpdate(%this); }
function DefendEffectBehavior::getDamageType(%this, %a, %d) { return DamageEffectBehavior::getDamageType(%this, %a, %d); }
function DefendEffectBehavior::getDamageMod(%this, %a, %d, %t) { return DamageEffectBehavior::getDamageMod(%this, %a, %d, %t); }
function DefendEffectBehavior::getExtraDamage(%this, %a, %d, %t) { return DamageEffectBehavior::getExtraDamage(%this, %a, %d, %t); }
function DefendEffectBehavior::getImpulse(%this, %a, %d, %t, %amnt) { return DamageEffectBehavior::getImpulse(%this, %a, %d, %t, %amnt); }
function DefendEffectBehavior::onDamage(%this, %a, %d, %t, %amnt, %imp) { return DamageEffectBehavior::onDamage(%this, %a, %d, %t, %amnt, %imp); }

