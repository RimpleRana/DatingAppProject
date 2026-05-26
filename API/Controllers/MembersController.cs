using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Extensions;
using API.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Controllers
{
    [Authorize]
    public class MembersController(IMemberRepository memberRepository, IPhotoService photoService) : BaseApiController
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

        [HttpPost("add-photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPhoto([FromForm] AddPhotoDto dto)
        {
            var member = await memberRepository.GetMemberUpdate(User.GetMemberId());

            if (member == null)
                return BadRequest("Cannot update member");

            var result = await photoService.UploadPhotoAsync(dto.File);

            if (result.Error != null)
                return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                MemberId = User.GetMemberId()
            };

            if (member.ImageUrl == null)
            {
                member.ImageUrl = photo.Url;
                member.User.ImageUrl = photo.Url;
            }

            member.Photos.Add(photo);

            if (await memberRepository.SaveAllAsync()) return Ok(photo);

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<IActionResult> SetMainPhoto(int photoId)
        {
            var member = await memberRepository.GetMemberUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Cannot get member from token");

            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);

            if(member.ImageUrl == photo?.Url || photo == null)
            {
                return BadRequest("Cannot set this as main image");
            }
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;

            if (await memberRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var member = await memberRepository.GetMemberUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Cannot get member from token");

            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);

            if(photo == null || photo.Url == member.ImageUrl)
            {
                return BadRequest("This photo cannot be deleted");
            }

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            member.Photos.Remove(photo);

            if (await memberRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the photo");
        }
    }
}
