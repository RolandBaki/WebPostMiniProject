using DyntellProject.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DyntellProject.Infrastructure.Services;

public interface IAuthService
{
    Task<(LoginResponseDto? result, string[]? errors)> RegisterAsync(RegisterDto registerDto);
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
}
