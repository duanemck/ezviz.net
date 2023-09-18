using ezviz.net;
using ezviz_mqtt.commands.publish_state;
using ezviz_mqtt.commands.state_updates;
using ezviz_mqtt.config;
using ezviz_mqtt.util;
using Microsoft.Extensions.Options;

namespace ezviz_mqtt.commands
{

    internal class StateCommandFactory : IStateCommandFactory
    {
        private readonly Dictionary<string, IStateUpdateCommand> StateUpdateCommands;
        private readonly Dictionary<string, IStatePublishCommand> StatePublishCommands;

        public StateCommandFactory(IEzvizClient client, TopicExtensions topics, IMqttHandler mqttHandler, IOptions<JsonOptions> jsonOptions)
        {
            var booleanConverter = new BooleanConvertor(jsonOptions.Value);
            StatePublishCommands = new Dictionary<string, IStatePublishCommand>(StringComparer.OrdinalIgnoreCase)
            {
               // { StateEntities.Image, new CameraImageCommand(topics,booleanConverter,mqttHandler,client) },
                { StateEntities.AlarmDetectionMethod, new AlarmDetectionMethodCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.AlarmScheduleEnabled, new AlarmScheduleEnabledCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.AlarmSound, new AlarmSoundCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.Armed, new ArmedCommand(topics,booleanConverter, mqttHandler, client    ) },
                { StateEntities.AudioEnabled, new AudioEnabledCommand(topics,booleanConverter, mqttHandler, client  ) },
                { StateEntities.BatteryLevel, new BatteryLevelCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.DetectionSensitivityLevel, new DetectionSensitivityCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.DiskCapacity, new DiskCapacityCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.InfraredEnabled, new InfraredEnabledCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.LastAlarm, new LastAlarmCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.LastAlarmImageUrl, new LastAlarmImageUrlCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.LastAlarmImage, new LastAlarmImageCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.MobileTrackingEnabled, new MobileTrackingEnabledCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.NotifyWhenOffline, new NotifyWhenOfflineCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.PirStatus, new PirStatusCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.RtspEncrypted, new RtspEncryptedCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.Sleeping, new SleepingCommand(topics,booleanConverter, mqttHandler, client  ) },
                { StateEntities.StatusLedEnabled, new StatusLedEnabledCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.TriggerAlarm, new TriggerAlarmStatusCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.UpgradeAvailable, new UpgradeAvailableCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.UpgradeInProgress, new UpgradeInProgressCommand(topics,booleanConverter, mqttHandler, client) },
                { StateEntities.UpgradePercent, new UpgradePercentCommand(topics,booleanConverter, mqttHandler, client) },
            };

            StateUpdateCommands = new Dictionary<string, IStateUpdateCommand>(StringComparer.OrdinalIgnoreCase)
            {
                { StateEntities.AlarmDetectionMethod, new AlarmDetectionMethodUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.AlarmDetectionMethod]) },
                { StateEntities.AlarmSound, new AlarmSoundUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.AlarmSound]) },
                { StateEntities.Armed, new ArmDisarmCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.Armed]) },
                { StateEntities.AudioEnabled, new AudioEnabledUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.AudioEnabled]) },
                { StateEntities.DetectionSensitivityLevel, new DetectionSensitivityUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.DetectionSensitivityLevel]) },
                { StateEntities.InfraredEnabled, new InfraredEnabledUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.InfraredEnabled]) },
                { StateEntities.MobileTrackingEnabled, new MobileTrackingEnabledUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.MobileTrackingEnabled]) },
                { StateEntities.Sleeping, new SleepingUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.Sleeping]) },
                { StateEntities.StatusLedEnabled, new StatusLedEnabledUpdateCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.StatusLedEnabled]) },
                { StateEntities.TriggerAlarm, new TriggerAlarmCommand(client, jsonOptions.Value, StatePublishCommands[StateEntities.TriggerAlarm]) },
                
            };

            
        }

        public IStateUpdateCommand GetStateUpdateCommand(string commandKey)
        {
            var command = StateUpdateCommands.ContainsKey(commandKey) ? StateUpdateCommands[commandKey] : null;
            if (command == null)
            {
                throw new Exception($"Unknown state update command [{commandKey}]");
            }
            return command;
        }

        public IStatePublishCommand GetStatePublishCommand(string commandKey)
        {
            var command = StatePublishCommands.ContainsKey(commandKey) ? StatePublishCommands[commandKey] : null;
            if (command == null)
            {
                throw new Exception($"Unknown state publish command [{commandKey}]");
            }
            return command;
        }

        public IEnumerable<IStatePublishCommand> GetAllStatePublishCommands()
        {
            return StatePublishCommands.Values;
        }
    }
}
