using System.Collections.Generic;
using APIClient.Configuration.Interfaces;
using FileIOHelper;

namespace APIClient.Configuration
{
    public class APISetupContainer : IAPISetupContainer
    {
        private readonly Dictionary<string, IAPISetup> _setups;
        public IReadOnlyDictionary<string, IAPISetup> Setups => _setups;

        public APISetupContainer(Dictionary<string, IIOHelper> setupFiles)
        {
            _setups = new Dictionary<string, IAPISetup>();

            foreach (var setup in setupFiles)
            {
                _setups[setup.Key] = new APISetup(setup.Value, setup.Key);
            }
        }
        
        public IAPISetup GetSetup(string setupName)
        {
            if (!_setups.TryGetValue(setupName, out var setup))
            {
                throw new KeyNotFoundException($"Setup '{setupName}' not found.");
            }
            
            return setup;
        }

        public void UpdateSetup(string setupName, IAPIConnectionInfo connectionInfo)
        {
            if (!_setups.TryGetValue(setupName, out var setup))
            {
                throw new KeyNotFoundException($"Setup '{setupName}' not found.");
            }
            
            setup.UpdateConnectionInfo(connectionInfo);
        }
    }
}