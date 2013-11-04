using System;
using System.Collections.Generic;
using DragonMarble.Account;

namespace DragonMarble.Card
{
    public class CharacterCardInfo
    {
        public Guid Id { get; set; }
        public GameItemGrade Grade
        {
            get { return _stat.Grade; }
        }
        public int Exp { get; set; }

        private CharacterCardStat _stat;
        public GameAccountInfo Owner { get; set; }
        public List<FortuneItem> Equipments { get; set; }
    }
}