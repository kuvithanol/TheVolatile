using UnityEngine;

namespace TheVolatile
{
    // Plays a small "cutscene" at the start of the game
    internal class SlimeStart : UpdatableAndDeletable
    {
        private Player Slime => (room.game.Players.Count <= 0) ? null : (room.game.Players[0].realizedCreature as Player);
        private int timer = 0;
        private StartController startController;

        public SlimeStart(Room room)
        {
            this.room = room;
        }

        public override void Update(bool eu)
        {
            Player ply = Slime;
            if (ply == null) return;
            if (room.game.cameras[0].room != room) return;
            startController = new StartController(this);

            // Spawn the player at the correct place
            if (timer == 0)
            {
                room.game.cameras[0].MoveCamera(1);

                ply.controller = startController;
                room.game.cameras[0].followAbstractCreature = null;

                if (room.game.cameras[0].hud == null)
                    room.game.cameras[0].FireUpSinglePlayerHUD(ply);

                for (int i = 0; i < 2; i++)
                {
                    ply.bodyChunks[i].HardSetPosition(room.MiddleOfTile(116, 25));
                }

                Lighter.getMine(ply).firstChunk.HardSetPosition(room.MiddleOfTile(124, 34));
                ply.playerState.foodInStomach = 1;
            }

            // End the cutscene
            if (timer == 360)
            {
                ply.controller = null;
                ply.room.game.cameras[0].followAbstractCreature = ply.abstractCreature;
                Destroy();
            }

            timer++;
        }

        // Makes Sprinter climb a pole without player input
        public class StartController : Player.PlayerController
        {
            public SlimeStart owner;

            public StartController(SlimeStart owner)
            {
                this.owner = owner;
            }

            public override Player.InputPackage GetInput()
            {
                return new Player.InputPackage(base.GetInput().gamePad, owner.timer > 80 && owner.timer < 40 ? -1 : 0, owner.timer > 340 ? 1 : 0, false, false, false, false, false);
            }
        }
    }
}