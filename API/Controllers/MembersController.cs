using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Extensions;

namespace API.Controllers
{
    [Authorize]
    public class MembersController(IMemberRepository memberRepository) : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetMembers()
        {
            return Ok(await memberRepository.GetMembersAsync());
        }
        
        [HttpGet("{id}")] // localhost:5001/api/members/bob-id
        public async Task<IActionResult> GetMember(string id)
        {
            var member = await memberRepository.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return Ok(member);
        }

        [HttpGet("{id}/photos")]
        public async Task<IActionResult> GetMemberPhotos(string id)
        {
            return Ok(await memberRepository.GetPhotosForMemberAsync(id));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var memberId = User.GetMemberId();

           var member = await memberRepository.GetMemberUpdate(memberId);
           if (member == null) return BadRequest("Member not found");

           member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
           member.Description = memberUpdateDto.Description ?? member.Description;
           member.City = memberUpdateDto.City ?? member.City;
           member.Country = memberUpdateDto.Country ?? member.Country;

           member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName;

          // memberRepository.Update(member); //optional

           if (await memberRepository.SaveAllAsync()) return NoContent();
           return BadRequest("Failed to update member");
        }
    }
}
