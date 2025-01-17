using System.Collections.Generic;

namespace APIClient.Configuration.Interfaces
{
    public interface IAPISetupContainer
    {
        /// <summary>
        /// API 설정 저장용 컨테이너
        /// </summary>
        IReadOnlyDictionary<string, IAPISetup> Setups { get; }
        /// <summary>
        /// 저장된 API 설정 가져오기
        /// </summary>
        /// <param name="setupName">API 설정 구분명</param>
        /// <returns><see cref="IAPISetup"/>: API 설정</returns>
        IAPISetup GetSetup(string setupName);
        /// <summary>
        /// 특정 API 설정 업데이트
        /// </summary>
        /// <param name="setupName">API 설정 구분명</param>
        /// <param name="connectionInfo"><see cref="IAPIConnectionInfo"/>: API 연결 세팅 모델</param>
        void UpdateSetup(string setupName, IAPIConnectionInfo connectionInfo);
    }
}