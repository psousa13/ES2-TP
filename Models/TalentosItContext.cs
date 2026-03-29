using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TalentosIT.Web.Models;

public partial class TalentosItContext : DbContext
{
    public TalentosItContext()
    {
    }

    public TalentosItContext(DbContextOptions<TalentosItContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Experiencium> Experiencia { get; set; }

    public virtual DbSet<PropostaSkill> PropostaSkills { get; set; }

    public virtual DbSet<PropostaTrabalho> PropostaTrabalhos { get; set; }

    public virtual DbSet<RegistoAtividade> RegistoAtividades { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Talento> Talentos { get; set; }

    public virtual DbSet<TalentoSkill> TalentoSkills { get; set; }

    public virtual DbSet<TalentosPublico> TalentosPublicos { get; set; }

    public virtual DbSet<Utilizador> Utilizadors { get; set; }

    public virtual DbSet<UtilizadoresAtivo> UtilizadoresAtivos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=TalentosIT;Username=postgres;Password=123456789");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum("tipo_utilizador", new[] { "utilizador", "gestor_utilizadores", "admin" });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("cliente_pkey");

            entity.ToTable("cliente");

            entity.HasIndex(e => e.Email, "cliente_email_key").IsUnique();

            entity.HasIndex(e => e.IdUtilizador, "idx_cliente_utilizador");

            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Apelido)
                .HasMaxLength(100)
                .HasColumnName("apelido");
            entity.Property(e => e.Cidade)
                .HasMaxLength(100)
                .HasColumnName("cidade");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.NumPorta)
                .HasMaxLength(10)
                .HasColumnName("num_porta");
            entity.Property(e => e.Pais)
                .HasMaxLength(100)
                .HasColumnName("pais");
            entity.Property(e => e.PrimeiroNome)
                .HasMaxLength(100)
                .HasColumnName("primeiro_nome");
            entity.Property(e => e.Rua)
                .HasMaxLength(255)
                .HasColumnName("rua");
            entity.Property(e => e.Telefone)
                .HasMaxLength(15)
                .HasColumnName("telefone");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdUtilizador)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("cliente_id_utilizador_fkey");
        });

        modelBuilder.Entity<Experiencium>(entity =>
        {
            entity.HasKey(e => e.IdExperiencia).HasName("experiencia_pkey");

            entity.ToTable("experiencia");

            entity.HasIndex(e => e.IdTalento, "idx_experiencia_talento");

            entity.Property(e => e.IdExperiencia).HasColumnName("id_experiencia");
            entity.Property(e => e.AnoFim).HasColumnName("ano_fim");
            entity.Property(e => e.AnoInicio).HasColumnName("ano_inicio");
            entity.Property(e => e.Empresa)
                .HasMaxLength(150)
                .HasColumnName("empresa");
            entity.Property(e => e.IdTalento).HasColumnName("id_talento");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdTalentoNavigation).WithMany(p => p.Experiencia)
                .HasForeignKey(d => d.IdTalento)
                .HasConstraintName("experiencia_id_talento_fkey");
        });

        modelBuilder.Entity<PropostaSkill>(entity =>
        {
            entity.HasKey(e => new { e.IdProposta, e.IdSkill }).HasName("proposta_skill_pkey");

            entity.ToTable("proposta_skill");

            entity.HasIndex(e => e.IdProposta, "idx_proposta_skill");

            entity.Property(e => e.IdProposta).HasColumnName("id_proposta");
            entity.Property(e => e.IdSkill).HasColumnName("id_skill");
            entity.Property(e => e.AnosMinimosExperiencia).HasColumnName("anos_minimos_experiencia");

            entity.HasOne(d => d.IdPropostaNavigation).WithMany(p => p.PropostaSkills)
                .HasForeignKey(d => d.IdProposta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_skill_id_proposta_fkey");

            entity.HasOne(d => d.IdSkillNavigation).WithMany(p => p.PropostaSkills)
                .HasForeignKey(d => d.IdSkill)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_skill_id_skill_fkey");
        });

        modelBuilder.Entity<PropostaTrabalho>(entity =>
        {
            entity.HasKey(e => e.IdProposta).HasName("proposta_trabalho_pkey");

            entity.ToTable("proposta_trabalho");

            entity.HasIndex(e => e.IdCliente, "idx_proposta_cliente");

            entity.HasIndex(e => e.IdUtilizador, "idx_proposta_utilizador");

            entity.Property(e => e.IdProposta).HasColumnName("id_proposta");
            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .HasColumnName("categoria");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.HorasTotais).HasColumnName("horas_totais");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .HasColumnName("titulo");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.PropostaTrabalhos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_trabalho_id_cliente_fkey");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.PropostaTrabalhos)
                .HasForeignKey(d => d.IdUtilizador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposta_trabalho_id_utilizador_fkey");
        });

        modelBuilder.Entity<RegistoAtividade>(entity =>
        {
            entity.HasKey(e => e.IdRegisto).HasName("registo_atividade_pkey");

            entity.ToTable("registo_atividade");

            entity.HasIndex(e => e.IdUtilizador, "idx_registo_utilizador");

            entity.Property(e => e.IdRegisto).HasColumnName("id_registo");
            entity.Property(e => e.DataHora)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("data_hora");
            entity.Property(e => e.DescricaoAcao).HasColumnName("descricao_acao");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.RegistoAtividades)
                .HasForeignKey(d => d.IdUtilizador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("registo_atividade_id_utilizador_fkey");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.IdSkill).HasName("skill_pkey");

            entity.ToTable("skill");

            entity.HasIndex(e => e.Nome, "skill_nome_key").IsUnique();

            entity.Property(e => e.IdSkill).HasColumnName("id_skill");
            entity.Property(e => e.AreaProfissional)
                .HasMaxLength(100)
                .HasColumnName("area_profissional");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<Talento>(entity =>
        {
            entity.HasKey(e => e.IdTalento).HasName("talento_pkey");

            entity.ToTable("talento");

            entity.HasIndex(e => e.IdUtilizador, "idx_talento_utilizador");

            entity.HasIndex(e => e.Email, "talento_email_key").IsUnique();

            entity.Property(e => e.IdTalento).HasColumnName("id_talento");
            entity.Property(e => e.Apelido)
                .HasMaxLength(100)
                .HasColumnName("apelido");
            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .HasColumnName("categoria");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.Pais)
                .HasMaxLength(100)
                .HasColumnName("pais");
            entity.Property(e => e.PrecoHora).HasColumnName("preco_hora");
            entity.Property(e => e.PrimeiroNome)
                .HasMaxLength(100)
                .HasColumnName("primeiro_nome");
            entity.Property(e => e.Publico)
                .HasDefaultValue(true)
                .HasColumnName("publico");
            entity.Property(e => e.Telefone)
                .HasMaxLength(15)
                .HasColumnName("telefone");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.Talentos)
                .HasForeignKey(d => d.IdUtilizador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("talento_id_utilizador_fkey");
        });

        modelBuilder.Entity<TalentoSkill>(entity =>
        {
            entity.HasKey(e => new { e.IdTalento, e.IdSkill }).HasName("talento_skill_pkey");

            entity.ToTable("talento_skill");

            entity.HasIndex(e => e.IdTalento, "idx_talento_skill");

            entity.Property(e => e.IdTalento).HasColumnName("id_talento");
            entity.Property(e => e.IdSkill).HasColumnName("id_skill");
            entity.Property(e => e.AnosExperiencia).HasColumnName("anos_experiencia");

            entity.HasOne(d => d.IdSkillNavigation).WithMany(p => p.TalentoSkills)
                .HasForeignKey(d => d.IdSkill)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("talento_skill_id_skill_fkey");

            entity.HasOne(d => d.IdTalentoNavigation).WithMany(p => p.TalentoSkills)
                .HasForeignKey(d => d.IdTalento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("talento_skill_id_talento_fkey");
        });

        modelBuilder.Entity<TalentosPublico>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("talentos_publicos");

            entity.Property(e => e.Apelido)
                .HasMaxLength(100)
                .HasColumnName("apelido");
            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .HasColumnName("categoria");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IdTalento).HasColumnName("id_talento");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.PrecoHora).HasColumnName("preco_hora");
            entity.Property(e => e.PrimeiroNome)
                .HasMaxLength(100)
                .HasColumnName("primeiro_nome");
            entity.Property(e => e.Publico).HasColumnName("publico");
            entity.Property(e => e.Telefone)
                .HasMaxLength(15)
                .HasColumnName("telefone");
        });

        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasKey(e => e.IdUtilizador).HasName("utilizador_pkey");

            entity.ToTable("utilizador");

            entity.HasIndex(e => e.Email, "utilizador_email_key").IsUnique();

            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.Apelido)
                .HasMaxLength(100)
                .HasColumnName("apelido");
            entity.Property(e => e.Ativo)
                .HasDefaultValue(true)
                .HasColumnName("ativo");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.PalavraPasse)
                .HasMaxLength(255)
                .HasColumnName("palavra_passe");
            entity.Property(e => e.PrimeiroNome)
                .HasMaxLength(100)
                .HasColumnName("primeiro_nome");
            entity.Property(e => e.Telefone)
                .HasMaxLength(15)
                .HasColumnName("telefone");
        });

        modelBuilder.Entity<UtilizadoresAtivo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("utilizadores_ativos");

            entity.Property(e => e.Apelido)
                .HasMaxLength(100)
                .HasColumnName("apelido");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.PalavraPasse)
                .HasMaxLength(255)
                .HasColumnName("palavra_passe");
            entity.Property(e => e.PrimeiroNome)
                .HasMaxLength(100)
                .HasColumnName("primeiro_nome");
            entity.Property(e => e.Telefone)
                .HasMaxLength(15)
                .HasColumnName("telefone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}