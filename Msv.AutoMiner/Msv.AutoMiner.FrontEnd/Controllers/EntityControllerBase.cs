using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public abstract class EntityControllerBase<TEntity, TEntityModel, TId> : Controller
        where TEntity : class, IEntity<TId>
    {
        private readonly string m_RowPartialView;
        private readonly AutoMinerDbContext m_Context;

        protected EntityControllerBase([AspMvcView] string rowPartialView, AutoMinerDbContext context)
        {
            m_RowPartialView = rowPartialView;
            m_Context = context;
        }

        [HttpPost]
        public virtual async Task<IActionResult> ToggleActive(TId id)
        {
            var entity = await m_Context.Set<TEntity>()
                .Where(x => x.Activity != ActivityState.Deleted)
                .FirstOrDefaultAsync(x => id.Equals(x.Id));
            if (entity == null)
                return NotFound();
            if (!IsEditable(entity))
                return Forbid();

            switch (entity.Activity)
            {
                case ActivityState.Active:
                    entity.Activity = ActivityState.Inactive;
                    break;
                case ActivityState.Inactive:
                    entity.Activity = ActivityState.Active;
                    break;
                default:
                    throw new InvalidOperationException("Deleted entities are not allowed here");
            }
            await m_Context.SaveChangesAsync();

            return PartialView(m_RowPartialView, GetEntityModels(new[] {id}).FirstOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Delete(TId id)
        {
            var entity = await m_Context.Set<TEntity>()
                .Where(x => x.Activity != ActivityState.Deleted)
                .FirstOrDefaultAsync(x => id.Equals(x.Id));
            if (entity == null)
                return NotFound();
            if (!IsEditable(entity))
                return Forbid();

            entity.Activity = ActivityState.Deleted;
            OnDeleting(entity);

            await m_Context.SaveChangesAsync();
            return NoContent();
        }

        protected IActionResult ReturnAsJsonFile(string fileName, string jsonContent)
            => File(Encoding.UTF8.GetBytes(jsonContent), "application/json", fileName);

        protected virtual bool IsEditable(TEntity entity) => true;

        protected virtual void OnDeleting(TEntity entity)
        { }

        protected abstract TEntityModel[] GetEntityModels(TId[] ids);
    }
}
