namespace APIClient.Configuration.Interfaces
{
    public interface IAPISetup
    {
        IAPIConnectionInfo ConnectionInfo { get; }
        /// <summary>
        /// API 연결 정보 가져오기
        /// </summary>
        /// <returns></returns>
        IAPIConnectionInfo GetConnectionInfo();
        /// <summary>
        /// API 연결 정보 업데이트
        /// </summary>
        /// <param name="connectionInfo"></param>
        void UpdateConnectionInfo(IAPIConnectionInfo connectionInfo);
    }
}