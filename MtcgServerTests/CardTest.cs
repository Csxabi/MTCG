using NUnit.Framework;
using System;
using MtcgServer.Cards.MonsterCards;
using MtcgServer.Cards.SpellCards;

namespace MtcgServerTests
{
    public class CardTest
    {
        [Test]
        public void TestFireVSWaterSpellCards()
        {
            var fire = new FireSpell() { Damage = 20 };
            var water = new WaterSpell() { Damage = 20 };

            int fireDamage = fire.CalculateDamage(water);
            int waterDamage = water.CalculateDamage(fire);

            // water > fire
            Assert.AreEqual(10, fireDamage);
            Assert.AreEqual(40, waterDamage);
        }

        [Test]
        public void TestFireVSNormalSpellCards()
        {
            var fire = new FireSpell() { Damage = 20 };
            var normal = new NormalSpell() { Damage = 20 };

            int fireDamage = fire.CalculateDamage(normal);
            int normalDamage = normal.CalculateDamage(fire);

            // fire > normal
            Assert.AreEqual(40, fireDamage);
            Assert.AreEqual(10, normalDamage);
        }

        [Test]
        public void TestNormalVSWaterSpellCards()
        {
            var normal = new NormalSpell() { Damage = 20 };
            var water = new WaterSpell() { Damage = 20 };

            int normalDamage = normal.CalculateDamage(water);
            int waterDamage = water.CalculateDamage(normal);

            // normal > water
            Assert.AreEqual(40, normalDamage);
            Assert.AreEqual(10, waterDamage);
        }

        [Test]
        public void TestPureMonsterFightDragonKraken()
        {
            var dragon = new Dragon() { Damage = 20 };
            var kraken = new Kraken() { Damage = 20 };

            int dragonDamage = dragon.CalculateDamage(kraken);
            int krakenDamage = kraken.CalculateDamage(dragon);

            // no element type effectiveness applies
            Assert.AreEqual(20, dragonDamage);
            Assert.AreEqual(20, krakenDamage);
        }

        [Test]
        public void TestPureMonsterFightWizardOrk()
        {
            var wizard = new Wizard() { Damage = 20 };
            var ork = new Ork() { Damage = 20 };

            int wizardDamage = wizard.CalculateDamage(ork);
            int orkDamage = ork.CalculateDamage(wizard);

            // no element type effectiveness applies
            // orks don't attack wizards
            Assert.AreEqual(20, wizardDamage);
            Assert.AreEqual(0, orkDamage);
        }

        [Test]
        public void TestPureMonsterFightFireElfDragon()
        {
            var elf = new FireElf() { Damage = 20 };
            var dragon = new Dragon() { Damage = 20 };

            int elfDamage = elf.CalculateDamage(dragon);
            int dragonDamage = dragon.CalculateDamage(elf);

            // no element type effectiveness applies
            // fire elves evade dragon attacks
            Assert.AreEqual(20, elfDamage);
            Assert.AreEqual(0, dragonDamage);
        }

        [Test]
        public void TestPureMonsterFightGoblinDragon()
        {
            var goblin = new Goblin() { Damage = 20 };
            var dragon = new Dragon() { Damage = 20 };

            int goblinDamage = goblin.CalculateDamage(dragon);
            int dragonDamage = dragon.CalculateDamage(goblin);

            // no element type effectiveness applies
            // goblins don't attack dragons
            Assert.AreEqual(0, goblinDamage);
            Assert.AreEqual(20, dragonDamage);
        }

        [Test]
        public void TestMixedFightKnightWaterSpell()
        {
            var knight = new Knight() { Damage = 20 };
            var water = new WaterSpell() { Damage = 20 };

            int knightDamage = knight.CalculateDamage(water);
            int waterDamage = water.CalculateDamage(knight);

            // normal > water
            // water spell drowns knights instantly
            Assert.AreEqual(20 * 2, knightDamage);
            Assert.AreEqual(9999 / 2, waterDamage);
        }

        [Test]
        public void TestMixedFightKrakenNormalSpell()
        {
            var kraken = new Kraken() { Damage = 20 };
            var normal = new NormalSpell() { Damage = 20 };

            int krakenDamage = kraken.CalculateDamage(normal);
            int normalDamage = normal.CalculateDamage(kraken);

            // normal > water
            // kraken not affected by spells
            Assert.AreEqual(10, krakenDamage);
            Assert.AreEqual(0, normalDamage);
        }

        [Test]
        public void TestMixedFightKrakenWaterSpell()
        {
            var kraken = new Kraken() { Damage = 20 };
            var water = new WaterSpell() { Damage = 20 };

            int krakenDamage = kraken.CalculateDamage(water);
            int waterDamage = water.CalculateDamage(kraken);

            // no element type effectiveness applies
            // kraken not affected by spells
            Assert.AreEqual(20, krakenDamage);
            Assert.AreEqual(0, waterDamage);
        }

        [Test]
        public void TestMixedFightKrakenFireSpell()
        {
            var kraken = new Kraken() { Damage = 20 };
            var fire = new FireSpell() { Damage = 20 };

            int krakenDamage = kraken.CalculateDamage(fire);
            int fireDamage = fire.CalculateDamage(kraken);

            // water > fire
            // kraken not affected by spells
            Assert.AreEqual(40, krakenDamage);
            Assert.AreEqual(0, fireDamage);
        }
    }
}
