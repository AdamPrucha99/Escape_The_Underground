using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeTheUnderground
{
    class Player
    {
        public double Health { get; set; }
        public int Ammo { get; set; }
        // Private vars
        private double baseHealth = 100;
        private int baseAmmo = 5;
        
        public void setBaseAmmo()
        {
            Ammo = baseAmmo;
        }
        public void setBaseHealth()
        {
            Health = baseHealth;
        }
        public void Damage(double damage)
        {
            Health -= damage;
        }
        public void Heal(int health)
        {
            Health += health;
        }
        public void Shoot()
        {
            Ammo -= 1;
        }

    }
}
