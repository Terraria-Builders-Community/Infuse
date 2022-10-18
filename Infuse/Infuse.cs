using Auxiliary.Configuration;
using Infuse.Data;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Infuse
{
    [ApiVersion(2, 1)]
    public class Infuse : TerrariaPlugin
    {
        public override string Author
            => "TBC Developers";

        public override string Description
            => "A plugin that gives users permanent buffs.";

        public override string Name
            => "Infuse";

        public override Version Version
            => new(1, 0);

        public Infuse(Main game)
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

            Commands.ChatCommands.Add(new Command("infuse.self", Self, "infuse"));
            Commands.ChatCommands.Add(new Command("infuse.others", Target, "infuseother"));
        }

        private void Self(CommandArgs args)
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

            Handle(args.Player, args.Parameters[0]);
        }

        private void Target(CommandArgs args)
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
                Handle(players[0], args.Parameters[1]);
        }

        private void Handle(TSPlayer source, string buff, TSPlayer? target = null)
        {
            target ??= source;

            var id = target.Account.ID;

            var entity = BuffsEntity.GetAsync(id).GetAwaiter().GetResult();

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

            if (buffId is > 0 and < Main.maxBuffTypes)
            {
                else if (entity.Buffs.Contains(buffId))
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