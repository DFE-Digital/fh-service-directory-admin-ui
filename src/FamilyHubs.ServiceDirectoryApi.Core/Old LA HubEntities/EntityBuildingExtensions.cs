//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using System.Linq.Expressions;

//namespace fh_service_directory_api.core.Entities;

//public static class EntityBuilderExtensions
//{
//    public static void HasEnum<TEntity, TProperty>
//    (
//        this EntityTypeBuilder<TEntity> entityBuilder,
//        Expression<Func<TEntity, TProperty>> propertyExpression
//    )
//    where TEntity : class
//    where TProperty : Enum
//    {
//        entityBuilder.Property(propertyExpression)
//            .HasConversion
//            (
//                v => v.ToString(),
//                v => (TProperty)Enum.Parse(typeof(TProperty), v)
//            );
//    }
//}
