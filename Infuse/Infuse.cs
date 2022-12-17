using Auxiliary;
using Auxiliary.Configuration;
using Infuse.Data;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Infuse
{
    [ApiVersion(2, 1)]
    public class Infuse : TerrariaPlugin
    {
        private System.Timers.Timer _buffTimer;

        public override string Author
            => "TBC Developers";

        public override string Description
            => "A plugin that gives users permanent buffs.";

        public override string Name
            => "Infuse";

        public override Version Version
            => new(1, 0);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Infuse(Main game)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            : base(game)
        {
            Order = 1;
        }

        public override void Initialize()
        {
            Configuration<InfuseSettings>.Load("Infuse");

            GeneralHooks.ReloadEvent += (x) =>
            {
                Configuration<InfuseSettings>.Load("Infuse");
                x.Player.SendSuccessMessage("[Infuse] Reloaded configuration.");
            };

            Commands.ChatCommands.Add(new Command("infuse.self",
                async (x) => await Self(x), "infuse"));

            Commands.ChatCommands.Add(new Command("infuse.others",
                async (x) => await Target(x), "infuseother"));

            _buffTimer = new(1000)
            {
                AutoReset = true
            };
            _buffTimer.Elapsed += async (_, x)
                => await Tick(x);

            _buffTimer.Start();
        }

        private static async Task Tick(ElapsedEventArgs _)
        {
            for (int i = 0; i < 255; i++)
            {
                var plr = TShock.Players[i];

                if (plr is null || !(plr.Active && plr.IsLoggedIn))
                    continue;

                if (plr.Account is null)
                    continue;

                var entity = await IModel.GetAsync(GetRequest.Bson<InfuseUser>(x => x.TShockId == plr.Account.ID), x => x.TShockId = plr.Account.ID);

                foreach (var buff in entity!.Buffs)
                    plr.SetBuff(buff, 120);
            }
        }

        private static async Task Self(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You cannot execute this command if you're not logged in.");
                return;
            }

            if (args.Parameters.Count is not 1)
            {
                args.Player.SendErrorMessage("Invalid syntax. Expected: '/infuse <buff>'");
                return;
            }

            await Handle(args.Player, args.Parameters[0]);
        }

        private static async Task Target(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("You cannot execute this command if you're not logged in.");
                return;
            }

            if (args.Parameters.Count is not 2)
            {
                args.Player.SendErrorMessage("Invalid syntax. Expected: '/infuseother <target> <buff>'");
                return;
            }

            var players = TSPlayer.FindByNameOrID(args.Parameters[0]);

            if (!players.Any())
                args.Player.SendErrorMessage("No player found to infuse.");

            else if (players.Count > 1)
                args.Player.SendMultipleMatchError(players.Select(x => x.Name));

            else
                await Handle(players[0], args.Parameters[1]);
        }

        private static async Task Handle(TSPlayer source, string buff, TSPlayer? target = null)
        {
            target ??= source;

            var id = target.Account.ID;

            var entity = await IModel.GetAsync(GetRequest.Bson<InfuseUser>(x => x.TShockId == target.Account.ID), x => x.TShockId = target.Account.ID);

            if (!int.TryParse(buff, out int buffId))
            {
                var found = TShock.Utils.GetBuffByName(buff);

                if (found.Count is 0)
                    source.SendErrorMessage("Invalid buff name!");

                else if (found.Count > 1)
                    source.SendMultipleMatchError(found.Select(f => Lang.GetBuffName(f)));

                else
                    buffId = found[0];

            }

            if (buffId is > 0 && buffId < Terraria.ID.BuffID.Count)
            {
                if (entity!.Buffs.Contains(buffId))
                {
                    entity.Buffs = entity.Buffs.Where(x => x != buffId).ToArray();

                    source.SendSuccessMessage("Succesfully removed buff.");

                    target.SendSuccessMessage($"Lost buff: {Lang.GetBuffName(buffId)}");
                }

                else
                {
                    if (Configuration<InfuseSettings>.Settings.BlacklistedBuffs.Contains(buffId) && !target.HasPermission("infuse.allowall"))
                    {
                        source.SendErrorMessage("This buff cannot be granted.");
                    }

                    entity.Buffs = entity.Buffs.Concat(new[] { buffId }).ToArray();

                    source.SendSuccessMessage("Succesfully added buff.");

                    target.SendSuccessMessage($"Gained buff: {Lang.GetBuffName(buffId)}");
                }
            }
            else
                source.SendErrorMessage("Invalid buff ID!");
        }
    }
}