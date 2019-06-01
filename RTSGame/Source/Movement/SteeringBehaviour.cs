﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSGame {

    public abstract class SteeringBehaviour {

        public Unit Target { get; protected set; }
        public int Weight { get; set; }

        public SteeringBehaviour() {
            Target = null;
            Weight = 1;
        }

        public abstract Steering GetSteering(Unit Unit);

        public abstract void SetTarget(Unit Target);
    }
}
