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
   %bt = new BehaviorTemplate();
   %bt.damageEffectClass = %name;
   %bt.damageEffectSuperClass = "AttackEffectBehavior";
   Damage.Effects.add(%bt);
   return %bt;
}

function Damage::createDefendEffect(%this, %name) {
   %bt = new BehaviorTemplate();
   %bt.damageEffectClass = %name;
   %bt.damageEffectSuperClass = "DefendEffectBehavior";
   Damage.Effects.add(%bt);
   return %bt;
}

function Damage::effect(%this, %obj, %effect) {
   %bi = %effect.createInstance();
   %bi.superClass = %effect.damageEffectSuperClass;
   %bi.class = %effect.damageEffectClass;
   %obj.addBehavior(%bi);
}

function Damage::everythingAttackedNear(%this, %attacker, %radius, %damage) {
   %defenders = %attacker.getScene().pickCircle(
      %attacker.getPosition(),
      %radius,
      "", "",
      Collision);
   for(%i = 0; %i < getWordCount(%defenders); %i++) {
      %defender = getWord(%defenders, %i);
      if(%defender == %attacker) continue;
      Damage.attack(%attacker, %defender, %damage);
   }
}

function Damage::just(%this, %defender, %dam) {
   %bs = "";
   for(%i = 0; %i < %defender.getBehaviorCount(); %i++) {
      if(%defender.getBehaviorByIndex(%i).superClass $= "DefendEffectBehavior") {
         %bs = %bs SPC %defender.getBehaviorByIndex(%i);
      }
   }

   %result = %this._damage("", %defender, %dam, %bs);

   %defender.onDamaged(%attacker, getWord(%result, 0), getWord(%result, 1), getWords(%result, 2, 3));
}

function Damage::attack(%this, %attacker, %defender, %dam) {
   // Get attack and defence behaviors.
   %attackB = "";
   for(%i = 0; %i < %attacker.getBehaviorCount(); %i++) {
      if(%attacker.getBehaviorByIndex(%i).superClass $= "AttackEffectBehavior") {
         %attackB = %attackB SPC %attacker.getBehaviorByIndex(%i);
      }
   }
   %defendB = "";
   for(%i = 0; %i < %defender.getBehaviorCount(); %i++) {
      if(%defender.getBehaviorByIndex(%i).superClass $= "DefendEffectBehavior") {
         %defendB = %defendB SPC %defender.getBehaviorByIndex(%i);
      }
   }
   %bs = trim(%attackB @ %defendB);

   %result = %this._damage(%attacker, %defender, %dam, %bs);

   // Let everyone know about the results.
   %attacker.onDamage(%defender, getWords(%result, 3), getWord(%result, 0), getWords(%result, 1, 2));
   %defender.onDamaged(%attacker, getWords(%result, 3), getWord(%result, 0), getWords(%result, 1, 2));
}

function Damage::_damage(%this, %attacker, %defender, %damage, %bs) {
   if(!%defender.isDamageEnabled) {
      return 0 SPC "" SPC "0 0";
   }

   // Parameters that will be filled in later.
   %types = "";
   %amount = 0;
   %impulse = "0 0";
   %mul = 1;
   %extra = 0;

   if(getWordCount(%damage) == 1 && strchr(%damage, ":") $= "") {
      %amount = %damage;
   } else {
      // Parse options.
   }

   // Get damage types from the behaviors.
   for(%i = 0; %i < getWordCount(%bs); %i++) {
      %b = getWord(%bs, %i);
      %types = %types SPC %b.getDamageType(%attacker, %defender);
   }
   // Get damage modifiers and extra given the type.
   for(%i = 0; %i < getWordCount(%bs); %i++) {
      %b = getWord(%bs, %i);
      %mul *= %b.getDamageMod(%attacker, %defender, %types); %extra += %b.getExtraDamage(%attacker, %defender, %types);
   }

   // Apply damage multiplier and extra damage.
   %amount = %amount * %mul + %extra;

   // Get impulse.
   for(%i = 0; %i < getWordCount(%bs); %i++) {
      %b = getWord(%bs, %i);
      %impulse = vector2Add(%impulse, %b.getImpulse(%attacker, %defender, %types, %amount));
   }

   // Apply the damage.
   %defender.damage += %amount;
   // Apply the impulse.
   //%defender.applyImpulse(%impulse, %defender.getPosition());

   return %amount SPC %impulse SPC %types;
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

function DamageEffectBehavior::onDamaged(%this, %attacker, %defender, %types, %amount, %impulse) {
}

// Ohmygosh.
function AttackEffectBehavior::onBehaviorAdd(%this) { DamageEffectBehavior::onBehaviorAdd(%this); }
function AttackEffectBehavior::onUpdate(%this) { DamageEffectBehavior::onUpdate(%this); }
function AttackEffectBehavior::getDamageType(%this, %a, %d) { return DamageEffectBehavior::getDamageType(%this, %a, %d); }
function AttackEffectBehavior::getDamageMod(%this, %a, %d, %t) { return DamageEffectBehavior::getDamageMod(%this, %a, %d, %t); }
function AttackEffectBehavior::getExtraDamage(%this, %a, %d, %t) { return DamageEffectBehavior::getExtraDamage(%this, %a, %d, %t); }
function AttackEffectBehavior::getImpulse(%this, %a, %d, %t, %amnt) { return DamageEffectBehavior::getImpulse(%this, %a, %d, %t, %amnt); }
function AttackEffectBehavior::onDamage(%this, %a, %d, %t, %amnt, %imp) { return DamageEffectBehavior::onDamage(%this, %a, %d, %t, %amnt, %imp); }
function AttackEffectBehavior::onDamaged(%this, %a, %d, %t, %amnt, %imp) { return DamageEffectBehavior::onDamaged(%this, %a, %d, %t, %amnt, %imp); }
function DefendEffectBehavior::onBehaviorAdd(%this) { DamageEffectBehavior::onBehaviorAdd(%this); }
function DefendEffectBehavior::onUpdate(%this) { DamageEffectBehavior::onUpdate(%this); }
function DefendEffectBehavior::getDamageType(%this, %a, %d) { return DamageEffectBehavior::getDamageType(%this, %a, %d); }
function DefendEffectBehavior::getDamageMod(%this, %a, %d, %t) { return DamageEffectBehavior::getDamageMod(%this, %a, %d, %t); }
function DefendEffectBehavior::getExtraDamage(%this, %a, %d, %t) { return DamageEffectBehavior::getExtraDamage(%this, %a, %d, %t); }
function DefendEffectBehavior::getImpulse(%this, %a, %d, %t, %amnt) { return DamageEffectBehavior::getImpulse(%this, %a, %d, %t, %amnt); }
function DefendEffectBehavior::onDamage(%this, %a, %d, %t, %amnt, %imp) { return DamageEffectBehavior::onDamage(%this, %a, %d, %t, %amnt, %imp); }
function DefendEffectBehavior::onDamaged(%this, %a, %d, %t, %amnt, %imp) { return DamageEffectBehavior::onDamaged(%this, %a, %d, %t, %amnt, %imp); }

