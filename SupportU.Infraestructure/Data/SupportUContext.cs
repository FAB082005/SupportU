using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Models;

namespace SupportU.Infraestructure.Data;

public partial class SupportUContext : DbContext
{
    public SupportUContext(DbContextOptions<SupportUContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asignacion> Asignacion { get; set; }

    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<Especialidad> Especialidad { get; set; }

    public virtual DbSet<Etiqueta> Etiqueta { get; set; }

    public virtual DbSet<HistorialEstado> HistorialEstado { get; set; }

    public virtual DbSet<Imagen> Imagen { get; set; }

    public virtual DbSet<Notificacion> Notificacion { get; set; }

    public virtual DbSet<Sla> Sla { get; set; }

    public virtual DbSet<Tecnico> Tecnico { get; set; }

    public virtual DbSet<Ticket> Ticket { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    public virtual DbSet<Valoracion> Valoracion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asignacion>(entity =>
        {
            entity.ToTable("ASIGNACION");

            entity.Property(e => e.AsignacionId).HasColumnName("asignacion_id");
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_asignacion");
            entity.Property(e => e.MetodoAsignacion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("metodo_asignacion");
            entity.Property(e => e.TecnicoId).HasColumnName("tecnico_id");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.UsuarioAsignadorId).HasColumnName("usuario_asignador_id");

            entity.HasOne(d => d.Tecnico).WithMany(p => p.Asignacion)
                .HasForeignKey(d => d.TecnicoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ASIGNACION_TECNICO");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Asignacion)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ASIGNACION_TICKET");

            entity.HasOne(d => d.UsuarioAsignador).WithMany(p => p.Asignacion)
                .HasForeignKey(d => d.UsuarioAsignadorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ASIGNACION_USUARIO_ASIGNADOR");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("CATEGORIA");

            entity.HasIndex(e => e.Nombre, "UQ_CATEGORIA_NOMBRE").IsUnique();

            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.CriterioAsignacion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("menor_carga")
                .HasColumnName("criterio_asignacion");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.SlaId).HasColumnName("sla_id");

            entity.HasOne(d => d.Sla).WithMany(p => p.Categoria)
                .HasForeignKey(d => d.SlaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CATEGORIA_SLA");

            entity.HasMany(d => d.Especialidad).WithMany(p => p.Categoria)
                .UsingEntity<Dictionary<string, object>>(
                    "CategoriaEspecialidad",
                    r => r.HasOne<Especialidad>().WithMany()
                        .HasForeignKey("EspecialidadId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_CATEGORIA_ESPECIALIDAD_ESPECIALIDAD"),
                    l => l.HasOne<Categoria>().WithMany()
                        .HasForeignKey("CategoriaId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_CATEGORIA_ESPECIALIDAD_CATEGORIA"),
                    j =>
                    {
                        j.HasKey("CategoriaId", "EspecialidadId");
                        j.ToTable("CATEGORIA_ESPECIALIDAD");
                        j.IndexerProperty<int>("CategoriaId").HasColumnName("categoria_id");
                        j.IndexerProperty<int>("EspecialidadId").HasColumnName("especialidad_id");
                    });
        });

        modelBuilder.Entity<Especialidad>(entity =>
        {
            entity.ToTable("ESPECIALIDAD");

            entity.HasIndex(e => e.Nombre, "UQ_ESPECIALIDAD_NOMBRE").IsUnique();

            entity.Property(e => e.EspecialidadId).HasColumnName("especialidad_id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Etiqueta>(entity =>
        {
            entity.ToTable("ETIQUETA");

            entity.HasIndex(e => e.CategoriaId, "IX_ETIQUETA_CATEGORIA");

            entity.HasIndex(e => e.Nombre, "UQ_ETIQUETA_NOMBRE").IsUnique();

            entity.Property(e => e.EtiquetaId).HasColumnName("etiqueta_id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Etiqueta)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ETIQUETA_CATEGORIA");
        });

        modelBuilder.Entity<HistorialEstado>(entity =>
        {
            entity.HasKey(e => e.HistorialId);

            entity.ToTable("HISTORIAL_ESTADO");

            entity.HasIndex(e => e.TicketId, "IX_HISTORIAL_TICKET");

            entity.Property(e => e.HistorialId).HasColumnName("historial_id");
            entity.Property(e => e.EstadoAnterior)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado_anterior");
            entity.Property(e => e.EstadoNuevo)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado_nuevo");
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_cambio");
            entity.Property(e => e.Observaciones)
                .HasColumnType("ntext")
                .HasColumnName("observaciones");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Ticket).WithMany(p => p.HistorialEstado)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HISTORIAL_ESTADO_TICKET");

            entity.HasOne(d => d.Usuario).WithMany(p => p.HistorialEstado)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HISTORIAL_ESTADO_USUARIO");
        });

        modelBuilder.Entity<Imagen>(entity =>
        {
            entity.ToTable("IMAGEN");

            entity.HasIndex(e => e.HistorialEstadoId, "IX_IMAGEN_HISTORIAL_ESTADO");

            entity.Property(e => e.ImagenId).HasColumnName("imagen_id");
            entity.Property(e => e.HistorialEstadoId).HasColumnName("historial_estado_id");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.RutaArchivo)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("ruta_archivo");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.HistorialEstado).WithMany(p => p.Imagenes)
                .HasForeignKey(d => d.HistorialEstadoId)
                .HasConstraintName("FK_IMAGEN_HISTORIAL_ESTADO");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Imagen)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IMAGEN_TICKET");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("NOTIFICACION");

            entity.HasIndex(e => new { e.UsuarioDestinatarioId, e.Estado }, "IX_NOTIFICACION_USUARIO");

            entity.Property(e => e.NotificacionId).HasColumnName("notificacion_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Mensaje)
                .HasColumnType("ntext")
                .HasColumnName("mensaje");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.TipoNotificacion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_notificacion");
            entity.Property(e => e.UsuarioDestinatarioId).HasColumnName("usuario_destinatario_id");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Notificacion)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_NOTIFICACION_TICKET");

            entity.HasOne(d => d.UsuarioDestinatario).WithMany(p => p.Notificacion)
                .HasForeignKey(d => d.UsuarioDestinatarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NOTIFICACION_USUARIO_DESTINATARIO");
        });

        modelBuilder.Entity<Sla>(entity =>
        {
            entity.ToTable("SLA");

            entity.HasIndex(e => e.Nombre, "UQ_SLA_NOMBRE").IsUnique();

            entity.Property(e => e.SlaId).HasColumnName("sla_id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.TiempoResolucionMinutos).HasColumnName("tiempo_resolucion_minutos");
            entity.Property(e => e.TiempoRespuestaMinutos).HasColumnName("tiempo_respuesta_minutos");
        });

        modelBuilder.Entity<Tecnico>(entity =>
        {
            entity.ToTable("TECNICO");

            entity.HasIndex(e => e.UsuarioId, "UQ_TECNICO_USUARIO_ID").IsUnique();

            entity.Property(e => e.TecnicoId).HasColumnName("tecnico_id");
            entity.Property(e => e.CalificacionPromedio)
                .HasDefaultValueSql("((0.00))")
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("calificacion_promedio");
            entity.Property(e => e.CargaTrabajo).HasColumnName("carga_trabajo");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Disponible")
                .HasColumnName("estado");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithOne(p => p.Tecnico)
                .HasForeignKey<Tecnico>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TECNICO_USUARIO");

            entity.HasMany(d => d.Especialidad).WithMany(p => p.Tecnico)
                .UsingEntity<Dictionary<string, object>>(
                    "TecnicoEspecialidad",
                    r => r.HasOne<Especialidad>().WithMany()
                        .HasForeignKey("EspecialidadId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TECNICO_ESPECIALIDAD_ESPECIALIDAD"),
                    l => l.HasOne<Tecnico>().WithMany()
                        .HasForeignKey("TecnicoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TECNICO_ESPECIALIDAD_TECNICO"),
                    j =>
                    {
                        j.HasKey("TecnicoId", "EspecialidadId");
                        j.ToTable("TECNICO_ESPECIALIDAD");
                        j.IndexerProperty<int>("TecnicoId").HasColumnName("tecnico_id");
                        j.IndexerProperty<int>("EspecialidadId").HasColumnName("especialidad_id");
                    });
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("TICKET");

            entity.HasIndex(e => e.CategoriaId, "IX_TICKET_CATEGORIA");

            entity.HasIndex(e => e.Estado, "IX_TICKET_ESTADO");

            entity.HasIndex(e => e.TecnicoAsignadoId, "IX_TICKET_TECNICO");

            entity.HasIndex(e => e.UsuarioSolicitanteId, "IX_TICKET_USUARIO_SOLICITANTE");

            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.CumplimientoResolucion).HasColumnName("cumplimiento_resolucion");
            entity.Property(e => e.CumplimientoRespuesta).HasColumnName("cumplimiento_respuesta");
            entity.Property(e => e.Descripcion)
                .HasColumnType("ntext")
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCierre).HasColumnName("fecha_cierre");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Prioridad)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Media")
                .HasColumnName("prioridad");
            entity.Property(e => e.TecnicoAsignadoId).HasColumnName("tecnico_asignado_id");
            entity.Property(e => e.Titulo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("titulo");
            entity.Property(e => e.UsuarioSolicitanteId).HasColumnName("usuario_solicitante_id");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Ticket)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TICKET_CATEGORIA");

            entity.HasOne(d => d.TecnicoAsignado).WithMany(p => p.Ticket)
                .HasForeignKey(d => d.TecnicoAsignadoId)
                .HasConstraintName("FK_TICKET_TECNICO_ASIGNADO");

            entity.HasOne(d => d.UsuarioSolicitante).WithMany(p => p.Ticket)
                .HasForeignKey(d => d.UsuarioSolicitanteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TICKET_USUARIO_SOLICITANTE");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.Email, "UQ_USUARIO_EMAIL").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidos");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("rol");
            entity.Property(e => e.UltimoInicioSesion).HasColumnName("ultimo_inicio_sesion");
        });

        modelBuilder.Entity<Valoracion>(entity =>
        {
            entity.ToTable("VALORACION");

            entity.HasIndex(e => e.TicketId, "UQ_VALORACION_TICKET_ID").IsUnique();

            entity.Property(e => e.ValoracionId).HasColumnName("valoracion_id");
            entity.Property(e => e.Comentario)
                .HasColumnType("ntext")
                .HasColumnName("comentario");
            entity.Property(e => e.FechaValoracion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_valoracion");
            entity.Property(e => e.Puntaje).HasColumnName("puntaje");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Ticket).WithOne(p => p.Valoracion)
                .HasForeignKey<Valoracion>(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VALORACION_TICKET");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Valoracion)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VALORACION_USUARIO");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
