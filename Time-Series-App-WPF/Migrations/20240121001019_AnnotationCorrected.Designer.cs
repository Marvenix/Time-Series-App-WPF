﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Time_Series_App_WPF.Context;

#nullable disable

namespace Time_Series_App_WPF.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240121001019_AnnotationCorrected")]
    partial class AnnotationCorrected
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.14");

            modelBuilder.Entity("Time_Series_App_WPF.Model.Annotation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Annotations");
                });
#pragma warning restore 612, 618
        }
    }
}
