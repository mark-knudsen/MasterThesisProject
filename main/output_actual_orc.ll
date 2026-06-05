; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr, i64 }
%struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType = type { ptr, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, i64, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8 }
%struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17 = type { ptr, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, i64, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8 }

@df = external global ptr
@__where_src = external global ptr
@str = private unnamed_addr constant [5 x i8] c"date\00", align 1
@str.1 = private unnamed_addr constant [9 x i8] c"latitude\00", align 1
@str.2 = private unnamed_addr constant [10 x i8] c"longitude\00", align 1
@str.3 = private unnamed_addr constant [15 x i8] c"wind-speed-min\00", align 1
@str.4 = private unnamed_addr constant [15 x i8] c"wind-speed-max\00", align 1
@str.5 = private unnamed_addr constant [16 x i8] c"wind-speed-mean\00", align 1
@str.6 = private unnamed_addr constant [19 x i8] c"wind-direction-min\00", align 1
@str.7 = private unnamed_addr constant [19 x i8] c"wind-direction-max\00", align 1
@str.8 = private unnamed_addr constant [20 x i8] c"wind-direction-mean\00", align 1
@str.9 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-min\00", align 1
@str.10 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-max\00", align 1
@str.11 = private unnamed_addr constant [29 x i8] c"surface-air-temperature-mean\00", align 1
@str.12 = private unnamed_addr constant [19 x i8] c"total-rainfall-sum\00", align 1
@str.13 = private unnamed_addr constant [21 x i8] c"surface-humidity-min\00", align 1
@str.14 = private unnamed_addr constant [21 x i8] c"surface-humidity-max\00", align 1
@str.15 = private unnamed_addr constant [22 x i8] c"surface-humidity-mean\00", align 1
@str.16 = private unnamed_addr constant [5 x i8] c"ndvi\00", align 1
@str.17 = private unnamed_addr constant [10 x i8] c"elevation\00", align 1
@str.18 = private unnamed_addr constant [6 x i8] c"slope\00", align 1
@str.19 = private unnamed_addr constant [7 x i8] c"aspect\00", align 1
@str.20 = private unnamed_addr constant [11 x i8] c"fire_label\00", align 1
@str.21 = private unnamed_addr constant [19 x i8] c"land_cover_class_1\00", align 1
@str.22 = private unnamed_addr constant [19 x i8] c"land_cover_class_2\00", align 1
@str.23 = private unnamed_addr constant [19 x i8] c"land_cover_class_4\00", align 1
@str.24 = private unnamed_addr constant [19 x i8] c"land_cover_class_5\00", align 1
@str.25 = private unnamed_addr constant [19 x i8] c"land_cover_class_6\00", align 1
@str.26 = private unnamed_addr constant [19 x i8] c"land_cover_class_7\00", align 1
@str.27 = private unnamed_addr constant [19 x i8] c"land_cover_class_8\00", align 1
@str.28 = private unnamed_addr constant [19 x i8] c"land_cover_class_9\00", align 1
@str.29 = private unnamed_addr constant [20 x i8] c"land_cover_class_10\00", align 1
@str.30 = private unnamed_addr constant [20 x i8] c"land_cover_class_11\00", align 1
@str.31 = private unnamed_addr constant [20 x i8] c"land_cover_class_12\00", align 1
@str.32 = private unnamed_addr constant [20 x i8] c"land_cover_class_13\00", align 1
@str.33 = private unnamed_addr constant [20 x i8] c"land_cover_class_14\00", align 1
@str.34 = private unnamed_addr constant [20 x i8] c"land_cover_class_15\00", align 1
@str.35 = private unnamed_addr constant [20 x i8] c"land_cover_class_16\00", align 1
@str.36 = private unnamed_addr constant [20 x i8] c"land_cover_class_17\00", align 1
@str.37 = private unnamed_addr constant [5 x i8] c"date\00", align 1
@str.38 = private unnamed_addr constant [9 x i8] c"latitude\00", align 1
@str.39 = private unnamed_addr constant [10 x i8] c"longitude\00", align 1
@str.40 = private unnamed_addr constant [15 x i8] c"wind-speed-min\00", align 1
@str.41 = private unnamed_addr constant [15 x i8] c"wind-speed-max\00", align 1
@str.42 = private unnamed_addr constant [16 x i8] c"wind-speed-mean\00", align 1
@str.43 = private unnamed_addr constant [19 x i8] c"wind-direction-min\00", align 1
@str.44 = private unnamed_addr constant [19 x i8] c"wind-direction-max\00", align 1
@str.45 = private unnamed_addr constant [20 x i8] c"wind-direction-mean\00", align 1
@str.46 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-min\00", align 1
@str.47 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-max\00", align 1
@str.48 = private unnamed_addr constant [29 x i8] c"surface-air-temperature-mean\00", align 1
@str.49 = private unnamed_addr constant [19 x i8] c"total-rainfall-sum\00", align 1
@str.50 = private unnamed_addr constant [21 x i8] c"surface-humidity-min\00", align 1
@str.51 = private unnamed_addr constant [21 x i8] c"surface-humidity-max\00", align 1
@str.52 = private unnamed_addr constant [22 x i8] c"surface-humidity-mean\00", align 1
@str.53 = private unnamed_addr constant [5 x i8] c"ndvi\00", align 1
@str.54 = private unnamed_addr constant [10 x i8] c"elevation\00", align 1
@str.55 = private unnamed_addr constant [6 x i8] c"slope\00", align 1
@str.56 = private unnamed_addr constant [7 x i8] c"aspect\00", align 1
@str.57 = private unnamed_addr constant [11 x i8] c"fire_label\00", align 1
@str.58 = private unnamed_addr constant [19 x i8] c"land_cover_class_1\00", align 1
@str.59 = private unnamed_addr constant [19 x i8] c"land_cover_class_2\00", align 1
@str.60 = private unnamed_addr constant [19 x i8] c"land_cover_class_4\00", align 1
@str.61 = private unnamed_addr constant [19 x i8] c"land_cover_class_5\00", align 1
@str.62 = private unnamed_addr constant [19 x i8] c"land_cover_class_6\00", align 1
@str.63 = private unnamed_addr constant [19 x i8] c"land_cover_class_7\00", align 1
@str.64 = private unnamed_addr constant [19 x i8] c"land_cover_class_8\00", align 1
@str.65 = private unnamed_addr constant [19 x i8] c"land_cover_class_9\00", align 1
@str.66 = private unnamed_addr constant [20 x i8] c"land_cover_class_10\00", align 1
@str.67 = private unnamed_addr constant [20 x i8] c"land_cover_class_11\00", align 1
@str.68 = private unnamed_addr constant [20 x i8] c"land_cover_class_12\00", align 1
@str.69 = private unnamed_addr constant [20 x i8] c"land_cover_class_13\00", align 1
@str.70 = private unnamed_addr constant [20 x i8] c"land_cover_class_14\00", align 1
@str.71 = private unnamed_addr constant [20 x i8] c"land_cover_class_15\00", align 1
@str.72 = private unnamed_addr constant [20 x i8] c"land_cover_class_16\00", align 1
@str.73 = private unnamed_addr constant [20 x i8] c"land_cover_class_17\00", align 1
@__where_result = external global ptr
@__where_i = external global i64
@0 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@1 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@2 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@3 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@4 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@5 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@6 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@7 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@8 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@9 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@10 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@11 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@12 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@13 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@14 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@15 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@16 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@17 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@18 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@19 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@20 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@21 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@22 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@23 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@24 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@25 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@26 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@27 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@28 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@29 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@30 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@31 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@32 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@33 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@34 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@35 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@36 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1
@37 = private unnamed_addr constant [5 x i8] c"OOB\0A\00", align 1

define ptr @main_17() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 800)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %0, align 8
  store i64 100, ptr %1, align 8
  store ptr %arr_data, ptr %2, align 8
  %arr_header1 = call ptr @malloc(i64 24)
  %arr_data2 = call ptr @malloc(i64 800)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 0
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 1
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 2
  store i64 0, ptr %3, align 8
  store i64 100, ptr %4, align 8
  store ptr %arr_data2, ptr %5, align 8
  %arr_header3 = call ptr @malloc(i64 24)
  %arr_data4 = call ptr @malloc(i64 800)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 0
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 1
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 2
  store i64 0, ptr %6, align 8
  store i64 100, ptr %7, align 8
  store ptr %arr_data4, ptr %8, align 8
  %arr_header5 = call ptr @malloc(i64 24)
  %arr_data6 = call ptr @malloc(i64 800)
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 0
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 1
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 2
  store i64 0, ptr %9, align 8
  store i64 100, ptr %10, align 8
  store ptr %arr_data6, ptr %11, align 8
  %arr_header7 = call ptr @malloc(i64 24)
  %arr_data8 = call ptr @malloc(i64 800)
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 0
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 1
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 2
  store i64 0, ptr %12, align 8
  store i64 100, ptr %13, align 8
  store ptr %arr_data8, ptr %14, align 8
  %arr_header9 = call ptr @malloc(i64 24)
  %arr_data10 = call ptr @malloc(i64 800)
  %15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header9, i32 0, i32 0
  %16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header9, i32 0, i32 1
  %17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header9, i32 0, i32 2
  store i64 0, ptr %15, align 8
  store i64 100, ptr %16, align 8
  store ptr %arr_data10, ptr %17, align 8
  %arr_header11 = call ptr @malloc(i64 24)
  %arr_data12 = call ptr @malloc(i64 800)
  %18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header11, i32 0, i32 0
  %19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header11, i32 0, i32 1
  %20 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header11, i32 0, i32 2
  store i64 0, ptr %18, align 8
  store i64 100, ptr %19, align 8
  store ptr %arr_data12, ptr %20, align 8
  %arr_header13 = call ptr @malloc(i64 24)
  %arr_data14 = call ptr @malloc(i64 800)
  %21 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header13, i32 0, i32 0
  %22 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header13, i32 0, i32 1
  %23 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header13, i32 0, i32 2
  store i64 0, ptr %21, align 8
  store i64 100, ptr %22, align 8
  store ptr %arr_data14, ptr %23, align 8
  %arr_header15 = call ptr @malloc(i64 24)
  %arr_data16 = call ptr @malloc(i64 800)
  %24 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 0
  %25 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 1
  %26 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 2
  store i64 0, ptr %24, align 8
  store i64 100, ptr %25, align 8
  store ptr %arr_data16, ptr %26, align 8
  %arr_header17 = call ptr @malloc(i64 24)
  %arr_data18 = call ptr @malloc(i64 800)
  %27 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header17, i32 0, i32 0
  %28 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header17, i32 0, i32 1
  %29 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header17, i32 0, i32 2
  store i64 0, ptr %27, align 8
  store i64 100, ptr %28, align 8
  store ptr %arr_data18, ptr %29, align 8
  %arr_header19 = call ptr @malloc(i64 24)
  %arr_data20 = call ptr @malloc(i64 800)
  %30 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header19, i32 0, i32 0
  %31 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header19, i32 0, i32 1
  %32 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header19, i32 0, i32 2
  store i64 0, ptr %30, align 8
  store i64 100, ptr %31, align 8
  store ptr %arr_data20, ptr %32, align 8
  %arr_header21 = call ptr @malloc(i64 24)
  %arr_data22 = call ptr @malloc(i64 800)
  %33 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header21, i32 0, i32 0
  %34 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header21, i32 0, i32 1
  %35 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header21, i32 0, i32 2
  store i64 0, ptr %33, align 8
  store i64 100, ptr %34, align 8
  store ptr %arr_data22, ptr %35, align 8
  %arr_header23 = call ptr @malloc(i64 24)
  %arr_data24 = call ptr @malloc(i64 800)
  %36 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header23, i32 0, i32 0
  %37 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header23, i32 0, i32 1
  %38 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header23, i32 0, i32 2
  store i64 0, ptr %36, align 8
  store i64 100, ptr %37, align 8
  store ptr %arr_data24, ptr %38, align 8
  %arr_header25 = call ptr @malloc(i64 24)
  %arr_data26 = call ptr @malloc(i64 800)
  %39 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header25, i32 0, i32 0
  %40 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header25, i32 0, i32 1
  %41 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header25, i32 0, i32 2
  store i64 0, ptr %39, align 8
  store i64 100, ptr %40, align 8
  store ptr %arr_data26, ptr %41, align 8
  %arr_header27 = call ptr @malloc(i64 24)
  %arr_data28 = call ptr @malloc(i64 800)
  %42 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header27, i32 0, i32 0
  %43 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header27, i32 0, i32 1
  %44 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header27, i32 0, i32 2
  store i64 0, ptr %42, align 8
  store i64 100, ptr %43, align 8
  store ptr %arr_data28, ptr %44, align 8
  %arr_header29 = call ptr @malloc(i64 24)
  %arr_data30 = call ptr @malloc(i64 800)
  %45 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header29, i32 0, i32 0
  %46 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header29, i32 0, i32 1
  %47 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header29, i32 0, i32 2
  store i64 0, ptr %45, align 8
  store i64 100, ptr %46, align 8
  store ptr %arr_data30, ptr %47, align 8
  %arr_header31 = call ptr @malloc(i64 24)
  %arr_data32 = call ptr @malloc(i64 800)
  %48 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header31, i32 0, i32 0
  %49 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header31, i32 0, i32 1
  %50 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header31, i32 0, i32 2
  store i64 0, ptr %48, align 8
  store i64 100, ptr %49, align 8
  store ptr %arr_data32, ptr %50, align 8
  %arr_header33 = call ptr @malloc(i64 24)
  %arr_data34 = call ptr @malloc(i64 800)
  %51 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header33, i32 0, i32 0
  %52 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header33, i32 0, i32 1
  %53 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header33, i32 0, i32 2
  store i64 0, ptr %51, align 8
  store i64 100, ptr %52, align 8
  store ptr %arr_data34, ptr %53, align 8
  %arr_header35 = call ptr @malloc(i64 24)
  %arr_data36 = call ptr @malloc(i64 800)
  %54 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header35, i32 0, i32 0
  %55 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header35, i32 0, i32 1
  %56 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header35, i32 0, i32 2
  store i64 0, ptr %54, align 8
  store i64 100, ptr %55, align 8
  store ptr %arr_data36, ptr %56, align 8
  %arr_header37 = call ptr @malloc(i64 24)
  %arr_data38 = call ptr @malloc(i64 800)
  %57 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 0
  %58 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 1
  %59 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 2
  store i64 0, ptr %57, align 8
  store i64 100, ptr %58, align 8
  store ptr %arr_data38, ptr %59, align 8
  %arr_header39 = call ptr @malloc(i64 24)
  %arr_data40 = call ptr @malloc(i64 800)
  %60 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 0
  %61 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 1
  %62 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 2
  store i64 0, ptr %60, align 8
  store i64 100, ptr %61, align 8
  store ptr %arr_data40, ptr %62, align 8
  %arr_header41 = call ptr @malloc(i64 24)
  %arr_data42 = call ptr @malloc(i64 100)
  %63 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header41, i32 0, i32 0
  %64 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header41, i32 0, i32 1
  %65 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header41, i32 0, i32 2
  store i64 0, ptr %63, align 8
  store i64 100, ptr %64, align 8
  store ptr %arr_data42, ptr %65, align 8
  %arr_header43 = call ptr @malloc(i64 24)
  %arr_data44 = call ptr @malloc(i64 100)
  %66 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header43, i32 0, i32 0
  %67 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header43, i32 0, i32 1
  %68 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header43, i32 0, i32 2
  store i64 0, ptr %66, align 8
  store i64 100, ptr %67, align 8
  store ptr %arr_data44, ptr %68, align 8
  %arr_header45 = call ptr @malloc(i64 24)
  %arr_data46 = call ptr @malloc(i64 100)
  %69 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header45, i32 0, i32 0
  %70 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header45, i32 0, i32 1
  %71 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header45, i32 0, i32 2
  store i64 0, ptr %69, align 8
  store i64 100, ptr %70, align 8
  store ptr %arr_data46, ptr %71, align 8
  %arr_header47 = call ptr @malloc(i64 24)
  %arr_data48 = call ptr @malloc(i64 100)
  %72 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header47, i32 0, i32 0
  %73 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header47, i32 0, i32 1
  %74 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header47, i32 0, i32 2
  store i64 0, ptr %72, align 8
  store i64 100, ptr %73, align 8
  store ptr %arr_data48, ptr %74, align 8
  %arr_header49 = call ptr @malloc(i64 24)
  %arr_data50 = call ptr @malloc(i64 100)
  %75 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header49, i32 0, i32 0
  %76 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header49, i32 0, i32 1
  %77 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header49, i32 0, i32 2
  store i64 0, ptr %75, align 8
  store i64 100, ptr %76, align 8
  store ptr %arr_data50, ptr %77, align 8
  %arr_header51 = call ptr @malloc(i64 24)
  %arr_data52 = call ptr @malloc(i64 100)
  %78 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header51, i32 0, i32 0
  %79 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header51, i32 0, i32 1
  %80 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header51, i32 0, i32 2
  store i64 0, ptr %78, align 8
  store i64 100, ptr %79, align 8
  store ptr %arr_data52, ptr %80, align 8
  %arr_header53 = call ptr @malloc(i64 24)
  %arr_data54 = call ptr @malloc(i64 100)
  %81 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header53, i32 0, i32 0
  %82 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header53, i32 0, i32 1
  %83 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header53, i32 0, i32 2
  store i64 0, ptr %81, align 8
  store i64 100, ptr %82, align 8
  store ptr %arr_data54, ptr %83, align 8
  %arr_header55 = call ptr @malloc(i64 24)
  %arr_data56 = call ptr @malloc(i64 100)
  %84 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header55, i32 0, i32 0
  %85 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header55, i32 0, i32 1
  %86 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header55, i32 0, i32 2
  store i64 0, ptr %84, align 8
  store i64 100, ptr %85, align 8
  store ptr %arr_data56, ptr %86, align 8
  %arr_header57 = call ptr @malloc(i64 24)
  %arr_data58 = call ptr @malloc(i64 100)
  %87 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header57, i32 0, i32 0
  %88 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header57, i32 0, i32 1
  %89 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header57, i32 0, i32 2
  store i64 0, ptr %87, align 8
  store i64 100, ptr %88, align 8
  store ptr %arr_data58, ptr %89, align 8
  %arr_header59 = call ptr @malloc(i64 24)
  %arr_data60 = call ptr @malloc(i64 100)
  %90 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header59, i32 0, i32 0
  %91 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header59, i32 0, i32 1
  %92 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header59, i32 0, i32 2
  store i64 0, ptr %90, align 8
  store i64 100, ptr %91, align 8
  store ptr %arr_data60, ptr %92, align 8
  %arr_header61 = call ptr @malloc(i64 24)
  %arr_data62 = call ptr @malloc(i64 100)
  %93 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header61, i32 0, i32 0
  %94 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header61, i32 0, i32 1
  %95 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header61, i32 0, i32 2
  store i64 0, ptr %93, align 8
  store i64 100, ptr %94, align 8
  store ptr %arr_data62, ptr %95, align 8
  %arr_header63 = call ptr @malloc(i64 24)
  %arr_data64 = call ptr @malloc(i64 100)
  %96 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header63, i32 0, i32 0
  %97 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header63, i32 0, i32 1
  %98 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header63, i32 0, i32 2
  store i64 0, ptr %96, align 8
  store i64 100, ptr %97, align 8
  store ptr %arr_data64, ptr %98, align 8
  %arr_header65 = call ptr @malloc(i64 24)
  %arr_data66 = call ptr @malloc(i64 100)
  %99 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header65, i32 0, i32 0
  %100 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header65, i32 0, i32 1
  %101 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header65, i32 0, i32 2
  store i64 0, ptr %99, align 8
  store i64 100, ptr %100, align 8
  store ptr %arr_data66, ptr %101, align 8
  %arr_header67 = call ptr @malloc(i64 24)
  %arr_data68 = call ptr @malloc(i64 100)
  %102 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header67, i32 0, i32 0
  %103 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header67, i32 0, i32 1
  %104 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header67, i32 0, i32 2
  store i64 0, ptr %102, align 8
  store i64 100, ptr %103, align 8
  store ptr %arr_data68, ptr %104, align 8
  %arr_header69 = call ptr @malloc(i64 24)
  %arr_data70 = call ptr @malloc(i64 100)
  %105 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header69, i32 0, i32 0
  %106 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header69, i32 0, i32 1
  %107 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header69, i32 0, i32 2
  store i64 0, ptr %105, align 8
  store i64 100, ptr %106, align 8
  store ptr %arr_data70, ptr %107, align 8
  %arr_header71 = call ptr @malloc(i64 24)
  %arr_data72 = call ptr @malloc(i64 100)
  %108 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header71, i32 0, i32 0
  %109 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header71, i32 0, i32 1
  %110 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header71, i32 0, i32 2
  store i64 0, ptr %108, align 8
  store i64 100, ptr %109, align 8
  store ptr %arr_data72, ptr %110, align 8
  %arr_header73 = call ptr @malloc(i64 24)
  %arr_data74 = call ptr @malloc(i64 296)
  %111 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header73, i32 0, i32 0
  %112 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header73, i32 0, i32 1
  %113 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header73, i32 0, i32 2
  store i64 37, ptr %111, align 8
  store i64 37, ptr %112, align 8
  store ptr %arr_data74, ptr %113, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data74, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr75 = getelementptr ptr, ptr %arr_data74, i64 1
  store ptr @str.1, ptr %elem_ptr75, align 8
  %elem_ptr76 = getelementptr ptr, ptr %arr_data74, i64 2
  store ptr @str.2, ptr %elem_ptr76, align 8
  %elem_ptr77 = getelementptr ptr, ptr %arr_data74, i64 3
  store ptr @str.3, ptr %elem_ptr77, align 8
  %elem_ptr78 = getelementptr ptr, ptr %arr_data74, i64 4
  store ptr @str.4, ptr %elem_ptr78, align 8
  %elem_ptr79 = getelementptr ptr, ptr %arr_data74, i64 5
  store ptr @str.5, ptr %elem_ptr79, align 8
  %elem_ptr80 = getelementptr ptr, ptr %arr_data74, i64 6
  store ptr @str.6, ptr %elem_ptr80, align 8
  %elem_ptr81 = getelementptr ptr, ptr %arr_data74, i64 7
  store ptr @str.7, ptr %elem_ptr81, align 8
  %elem_ptr82 = getelementptr ptr, ptr %arr_data74, i64 8
  store ptr @str.8, ptr %elem_ptr82, align 8
  %elem_ptr83 = getelementptr ptr, ptr %arr_data74, i64 9
  store ptr @str.9, ptr %elem_ptr83, align 8
  %elem_ptr84 = getelementptr ptr, ptr %arr_data74, i64 10
  store ptr @str.10, ptr %elem_ptr84, align 8
  %elem_ptr85 = getelementptr ptr, ptr %arr_data74, i64 11
  store ptr @str.11, ptr %elem_ptr85, align 8
  %elem_ptr86 = getelementptr ptr, ptr %arr_data74, i64 12
  store ptr @str.12, ptr %elem_ptr86, align 8
  %elem_ptr87 = getelementptr ptr, ptr %arr_data74, i64 13
  store ptr @str.13, ptr %elem_ptr87, align 8
  %elem_ptr88 = getelementptr ptr, ptr %arr_data74, i64 14
  store ptr @str.14, ptr %elem_ptr88, align 8
  %elem_ptr89 = getelementptr ptr, ptr %arr_data74, i64 15
  store ptr @str.15, ptr %elem_ptr89, align 8
  %elem_ptr90 = getelementptr ptr, ptr %arr_data74, i64 16
  store ptr @str.16, ptr %elem_ptr90, align 8
  %elem_ptr91 = getelementptr ptr, ptr %arr_data74, i64 17
  store ptr @str.17, ptr %elem_ptr91, align 8
  %elem_ptr92 = getelementptr ptr, ptr %arr_data74, i64 18
  store ptr @str.18, ptr %elem_ptr92, align 8
  %elem_ptr93 = getelementptr ptr, ptr %arr_data74, i64 19
  store ptr @str.19, ptr %elem_ptr93, align 8
  %elem_ptr94 = getelementptr ptr, ptr %arr_data74, i64 20
  store ptr @str.20, ptr %elem_ptr94, align 8
  %elem_ptr95 = getelementptr ptr, ptr %arr_data74, i64 21
  store ptr @str.21, ptr %elem_ptr95, align 8
  %elem_ptr96 = getelementptr ptr, ptr %arr_data74, i64 22
  store ptr @str.22, ptr %elem_ptr96, align 8
  %elem_ptr97 = getelementptr ptr, ptr %arr_data74, i64 23
  store ptr @str.23, ptr %elem_ptr97, align 8
  %elem_ptr98 = getelementptr ptr, ptr %arr_data74, i64 24
  store ptr @str.24, ptr %elem_ptr98, align 8
  %elem_ptr99 = getelementptr ptr, ptr %arr_data74, i64 25
  store ptr @str.25, ptr %elem_ptr99, align 8
  %elem_ptr100 = getelementptr ptr, ptr %arr_data74, i64 26
  store ptr @str.26, ptr %elem_ptr100, align 8
  %elem_ptr101 = getelementptr ptr, ptr %arr_data74, i64 27
  store ptr @str.27, ptr %elem_ptr101, align 8
  %elem_ptr102 = getelementptr ptr, ptr %arr_data74, i64 28
  store ptr @str.28, ptr %elem_ptr102, align 8
  %elem_ptr103 = getelementptr ptr, ptr %arr_data74, i64 29
  store ptr @str.29, ptr %elem_ptr103, align 8
  %elem_ptr104 = getelementptr ptr, ptr %arr_data74, i64 30
  store ptr @str.30, ptr %elem_ptr104, align 8
  %elem_ptr105 = getelementptr ptr, ptr %arr_data74, i64 31
  store ptr @str.31, ptr %elem_ptr105, align 8
  %elem_ptr106 = getelementptr ptr, ptr %arr_data74, i64 32
  store ptr @str.32, ptr %elem_ptr106, align 8
  %elem_ptr107 = getelementptr ptr, ptr %arr_data74, i64 33
  store ptr @str.33, ptr %elem_ptr107, align 8
  %elem_ptr108 = getelementptr ptr, ptr %arr_data74, i64 34
  store ptr @str.34, ptr %elem_ptr108, align 8
  %elem_ptr109 = getelementptr ptr, ptr %arr_data74, i64 35
  store ptr @str.35, ptr %elem_ptr109, align 8
  %elem_ptr110 = getelementptr ptr, ptr %arr_data74, i64 36
  store ptr @str.36, ptr %elem_ptr110, align 8
  %arr_header111 = call ptr @malloc(i64 24)
  %arr_data112 = call ptr @malloc(i64 296)
  %114 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header111, i32 0, i32 0
  %115 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header111, i32 0, i32 1
  %116 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header111, i32 0, i32 2
  store i64 37, ptr %114, align 8
  store i64 37, ptr %115, align 8
  store ptr %arr_data112, ptr %116, align 8
  %elem_ptr113 = getelementptr i64, ptr %arr_data112, i64 0
  store i64 4, ptr %elem_ptr113, align 8
  %elem_ptr114 = getelementptr i64, ptr %arr_data112, i64 1
  store i64 2, ptr %elem_ptr114, align 8
  %elem_ptr115 = getelementptr i64, ptr %arr_data112, i64 2
  store i64 2, ptr %elem_ptr115, align 8
  %elem_ptr116 = getelementptr i64, ptr %arr_data112, i64 3
  store i64 2, ptr %elem_ptr116, align 8
  %elem_ptr117 = getelementptr i64, ptr %arr_data112, i64 4
  store i64 2, ptr %elem_ptr117, align 8
  %elem_ptr118 = getelementptr i64, ptr %arr_data112, i64 5
  store i64 2, ptr %elem_ptr118, align 8
  %elem_ptr119 = getelementptr i64, ptr %arr_data112, i64 6
  store i64 2, ptr %elem_ptr119, align 8
  %elem_ptr120 = getelementptr i64, ptr %arr_data112, i64 7
  store i64 2, ptr %elem_ptr120, align 8
  %elem_ptr121 = getelementptr i64, ptr %arr_data112, i64 8
  store i64 2, ptr %elem_ptr121, align 8
  %elem_ptr122 = getelementptr i64, ptr %arr_data112, i64 9
  store i64 2, ptr %elem_ptr122, align 8
  %elem_ptr123 = getelementptr i64, ptr %arr_data112, i64 10
  store i64 2, ptr %elem_ptr123, align 8
  %elem_ptr124 = getelementptr i64, ptr %arr_data112, i64 11
  store i64 2, ptr %elem_ptr124, align 8
  %elem_ptr125 = getelementptr i64, ptr %arr_data112, i64 12
  store i64 2, ptr %elem_ptr125, align 8
  %elem_ptr126 = getelementptr i64, ptr %arr_data112, i64 13
  store i64 2, ptr %elem_ptr126, align 8
  %elem_ptr127 = getelementptr i64, ptr %arr_data112, i64 14
  store i64 2, ptr %elem_ptr127, align 8
  %elem_ptr128 = getelementptr i64, ptr %arr_data112, i64 15
  store i64 2, ptr %elem_ptr128, align 8
  %elem_ptr129 = getelementptr i64, ptr %arr_data112, i64 16
  store i64 2, ptr %elem_ptr129, align 8
  %elem_ptr130 = getelementptr i64, ptr %arr_data112, i64 17
  store i64 2, ptr %elem_ptr130, align 8
  %elem_ptr131 = getelementptr i64, ptr %arr_data112, i64 18
  store i64 2, ptr %elem_ptr131, align 8
  %elem_ptr132 = getelementptr i64, ptr %arr_data112, i64 19
  store i64 2, ptr %elem_ptr132, align 8
  %elem_ptr133 = getelementptr i64, ptr %arr_data112, i64 20
  store i64 1, ptr %elem_ptr133, align 8
  %elem_ptr134 = getelementptr i64, ptr %arr_data112, i64 21
  store i64 3, ptr %elem_ptr134, align 8
  %elem_ptr135 = getelementptr i64, ptr %arr_data112, i64 22
  store i64 3, ptr %elem_ptr135, align 8
  %elem_ptr136 = getelementptr i64, ptr %arr_data112, i64 23
  store i64 3, ptr %elem_ptr136, align 8
  %elem_ptr137 = getelementptr i64, ptr %arr_data112, i64 24
  store i64 3, ptr %elem_ptr137, align 8
  %elem_ptr138 = getelementptr i64, ptr %arr_data112, i64 25
  store i64 3, ptr %elem_ptr138, align 8
  %elem_ptr139 = getelementptr i64, ptr %arr_data112, i64 26
  store i64 3, ptr %elem_ptr139, align 8
  %elem_ptr140 = getelementptr i64, ptr %arr_data112, i64 27
  store i64 3, ptr %elem_ptr140, align 8
  %elem_ptr141 = getelementptr i64, ptr %arr_data112, i64 28
  store i64 3, ptr %elem_ptr141, align 8
  %elem_ptr142 = getelementptr i64, ptr %arr_data112, i64 29
  store i64 3, ptr %elem_ptr142, align 8
  %elem_ptr143 = getelementptr i64, ptr %arr_data112, i64 30
  store i64 3, ptr %elem_ptr143, align 8
  %elem_ptr144 = getelementptr i64, ptr %arr_data112, i64 31
  store i64 3, ptr %elem_ptr144, align 8
  %elem_ptr145 = getelementptr i64, ptr %arr_data112, i64 32
  store i64 3, ptr %elem_ptr145, align 8
  %elem_ptr146 = getelementptr i64, ptr %arr_data112, i64 33
  store i64 3, ptr %elem_ptr146, align 8
  %elem_ptr147 = getelementptr i64, ptr %arr_data112, i64 34
  store i64 3, ptr %elem_ptr147, align 8
  %elem_ptr148 = getelementptr i64, ptr %arr_data112, i64 35
  store i64 3, ptr %elem_ptr148, align 8
  %elem_ptr149 = getelementptr i64, ptr %arr_data112, i64 36
  store i64 3, ptr %elem_ptr149, align 8
  %data_header = call ptr @malloc(i64 32)
  %data_buffer = call ptr @malloc(i64 296)
  %117 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 0
  store i64 37, ptr %117, align 4
  %118 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 1
  store i64 37, ptr %118, align 4
  %119 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 2
  store ptr %data_buffer, ptr %119, align 8
  %data_gep = getelementptr ptr, ptr %data_buffer, i64 0
  store ptr %arr_header, ptr %data_gep, align 8
  %data_gep150 = getelementptr ptr, ptr %data_buffer, i64 1
  store ptr %arr_header1, ptr %data_gep150, align 8
  %data_gep151 = getelementptr ptr, ptr %data_buffer, i64 2
  store ptr %arr_header3, ptr %data_gep151, align 8
  %data_gep152 = getelementptr ptr, ptr %data_buffer, i64 3
  store ptr %arr_header5, ptr %data_gep152, align 8
  %data_gep153 = getelementptr ptr, ptr %data_buffer, i64 4
  store ptr %arr_header7, ptr %data_gep153, align 8
  %data_gep154 = getelementptr ptr, ptr %data_buffer, i64 5
  store ptr %arr_header9, ptr %data_gep154, align 8
  %data_gep155 = getelementptr ptr, ptr %data_buffer, i64 6
  store ptr %arr_header11, ptr %data_gep155, align 8
  %data_gep156 = getelementptr ptr, ptr %data_buffer, i64 7
  store ptr %arr_header13, ptr %data_gep156, align 8
  %data_gep157 = getelementptr ptr, ptr %data_buffer, i64 8
  store ptr %arr_header15, ptr %data_gep157, align 8
  %data_gep158 = getelementptr ptr, ptr %data_buffer, i64 9
  store ptr %arr_header17, ptr %data_gep158, align 8
  %data_gep159 = getelementptr ptr, ptr %data_buffer, i64 10
  store ptr %arr_header19, ptr %data_gep159, align 8
  %data_gep160 = getelementptr ptr, ptr %data_buffer, i64 11
  store ptr %arr_header21, ptr %data_gep160, align 8
  %data_gep161 = getelementptr ptr, ptr %data_buffer, i64 12
  store ptr %arr_header23, ptr %data_gep161, align 8
  %data_gep162 = getelementptr ptr, ptr %data_buffer, i64 13
  store ptr %arr_header25, ptr %data_gep162, align 8
  %data_gep163 = getelementptr ptr, ptr %data_buffer, i64 14
  store ptr %arr_header27, ptr %data_gep163, align 8
  %data_gep164 = getelementptr ptr, ptr %data_buffer, i64 15
  store ptr %arr_header29, ptr %data_gep164, align 8
  %data_gep165 = getelementptr ptr, ptr %data_buffer, i64 16
  store ptr %arr_header31, ptr %data_gep165, align 8
  %data_gep166 = getelementptr ptr, ptr %data_buffer, i64 17
  store ptr %arr_header33, ptr %data_gep166, align 8
  %data_gep167 = getelementptr ptr, ptr %data_buffer, i64 18
  store ptr %arr_header35, ptr %data_gep167, align 8
  %data_gep168 = getelementptr ptr, ptr %data_buffer, i64 19
  store ptr %arr_header37, ptr %data_gep168, align 8
  %data_gep169 = getelementptr ptr, ptr %data_buffer, i64 20
  store ptr %arr_header39, ptr %data_gep169, align 8
  %data_gep170 = getelementptr ptr, ptr %data_buffer, i64 21
  store ptr %arr_header41, ptr %data_gep170, align 8
  %data_gep171 = getelementptr ptr, ptr %data_buffer, i64 22
  store ptr %arr_header43, ptr %data_gep171, align 8
  %data_gep172 = getelementptr ptr, ptr %data_buffer, i64 23
  store ptr %arr_header45, ptr %data_gep172, align 8
  %data_gep173 = getelementptr ptr, ptr %data_buffer, i64 24
  store ptr %arr_header47, ptr %data_gep173, align 8
  %data_gep174 = getelementptr ptr, ptr %data_buffer, i64 25
  store ptr %arr_header49, ptr %data_gep174, align 8
  %data_gep175 = getelementptr ptr, ptr %data_buffer, i64 26
  store ptr %arr_header51, ptr %data_gep175, align 8
  %data_gep176 = getelementptr ptr, ptr %data_buffer, i64 27
  store ptr %arr_header53, ptr %data_gep176, align 8
  %data_gep177 = getelementptr ptr, ptr %data_buffer, i64 28
  store ptr %arr_header55, ptr %data_gep177, align 8
  %data_gep178 = getelementptr ptr, ptr %data_buffer, i64 29
  store ptr %arr_header57, ptr %data_gep178, align 8
  %data_gep179 = getelementptr ptr, ptr %data_buffer, i64 30
  store ptr %arr_header59, ptr %data_gep179, align 8
  %data_gep180 = getelementptr ptr, ptr %data_buffer, i64 31
  store ptr %arr_header61, ptr %data_gep180, align 8
  %data_gep181 = getelementptr ptr, ptr %data_buffer, i64 32
  store ptr %arr_header63, ptr %data_gep181, align 8
  %data_gep182 = getelementptr ptr, ptr %data_buffer, i64 33
  store ptr %arr_header65, ptr %data_gep182, align 8
  %data_gep183 = getelementptr ptr, ptr %data_buffer, i64 34
  store ptr %arr_header67, ptr %data_gep183, align 8
  %data_gep184 = getelementptr ptr, ptr %data_buffer, i64 35
  store ptr %arr_header69, ptr %data_gep184, align 8
  %data_gep185 = getelementptr ptr, ptr %data_buffer, i64 36
  store ptr %arr_header71, ptr %data_gep185, align 8
  %arr_header186 = call ptr @malloc(i64 24)
  %arr_data187 = call ptr @malloc(i64 296)
  %120 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header186, i32 0, i32 0
  %121 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header186, i32 0, i32 1
  %122 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header186, i32 0, i32 2
  store i64 37, ptr %120, align 8
  store i64 37, ptr %121, align 8
  store ptr %arr_data187, ptr %122, align 8
  %elem_ptr188 = getelementptr ptr, ptr %arr_data187, i64 0
  store ptr @str.37, ptr %elem_ptr188, align 8
  %elem_ptr189 = getelementptr ptr, ptr %arr_data187, i64 1
  store ptr @str.38, ptr %elem_ptr189, align 8
  %elem_ptr190 = getelementptr ptr, ptr %arr_data187, i64 2
  store ptr @str.39, ptr %elem_ptr190, align 8
  %elem_ptr191 = getelementptr ptr, ptr %arr_data187, i64 3
  store ptr @str.40, ptr %elem_ptr191, align 8
  %elem_ptr192 = getelementptr ptr, ptr %arr_data187, i64 4
  store ptr @str.41, ptr %elem_ptr192, align 8
  %elem_ptr193 = getelementptr ptr, ptr %arr_data187, i64 5
  store ptr @str.42, ptr %elem_ptr193, align 8
  %elem_ptr194 = getelementptr ptr, ptr %arr_data187, i64 6
  store ptr @str.43, ptr %elem_ptr194, align 8
  %elem_ptr195 = getelementptr ptr, ptr %arr_data187, i64 7
  store ptr @str.44, ptr %elem_ptr195, align 8
  %elem_ptr196 = getelementptr ptr, ptr %arr_data187, i64 8
  store ptr @str.45, ptr %elem_ptr196, align 8
  %elem_ptr197 = getelementptr ptr, ptr %arr_data187, i64 9
  store ptr @str.46, ptr %elem_ptr197, align 8
  %elem_ptr198 = getelementptr ptr, ptr %arr_data187, i64 10
  store ptr @str.47, ptr %elem_ptr198, align 8
  %elem_ptr199 = getelementptr ptr, ptr %arr_data187, i64 11
  store ptr @str.48, ptr %elem_ptr199, align 8
  %elem_ptr200 = getelementptr ptr, ptr %arr_data187, i64 12
  store ptr @str.49, ptr %elem_ptr200, align 8
  %elem_ptr201 = getelementptr ptr, ptr %arr_data187, i64 13
  store ptr @str.50, ptr %elem_ptr201, align 8
  %elem_ptr202 = getelementptr ptr, ptr %arr_data187, i64 14
  store ptr @str.51, ptr %elem_ptr202, align 8
  %elem_ptr203 = getelementptr ptr, ptr %arr_data187, i64 15
  store ptr @str.52, ptr %elem_ptr203, align 8
  %elem_ptr204 = getelementptr ptr, ptr %arr_data187, i64 16
  store ptr @str.53, ptr %elem_ptr204, align 8
  %elem_ptr205 = getelementptr ptr, ptr %arr_data187, i64 17
  store ptr @str.54, ptr %elem_ptr205, align 8
  %elem_ptr206 = getelementptr ptr, ptr %arr_data187, i64 18
  store ptr @str.55, ptr %elem_ptr206, align 8
  %elem_ptr207 = getelementptr ptr, ptr %arr_data187, i64 19
  store ptr @str.56, ptr %elem_ptr207, align 8
  %elem_ptr208 = getelementptr ptr, ptr %arr_data187, i64 20
  store ptr @str.57, ptr %elem_ptr208, align 8
  %elem_ptr209 = getelementptr ptr, ptr %arr_data187, i64 21
  store ptr @str.58, ptr %elem_ptr209, align 8
  %elem_ptr210 = getelementptr ptr, ptr %arr_data187, i64 22
  store ptr @str.59, ptr %elem_ptr210, align 8
  %elem_ptr211 = getelementptr ptr, ptr %arr_data187, i64 23
  store ptr @str.60, ptr %elem_ptr211, align 8
  %elem_ptr212 = getelementptr ptr, ptr %arr_data187, i64 24
  store ptr @str.61, ptr %elem_ptr212, align 8
  %elem_ptr213 = getelementptr ptr, ptr %arr_data187, i64 25
  store ptr @str.62, ptr %elem_ptr213, align 8
  %elem_ptr214 = getelementptr ptr, ptr %arr_data187, i64 26
  store ptr @str.63, ptr %elem_ptr214, align 8
  %elem_ptr215 = getelementptr ptr, ptr %arr_data187, i64 27
  store ptr @str.64, ptr %elem_ptr215, align 8
  %elem_ptr216 = getelementptr ptr, ptr %arr_data187, i64 28
  store ptr @str.65, ptr %elem_ptr216, align 8
  %elem_ptr217 = getelementptr ptr, ptr %arr_data187, i64 29
  store ptr @str.66, ptr %elem_ptr217, align 8
  %elem_ptr218 = getelementptr ptr, ptr %arr_data187, i64 30
  store ptr @str.67, ptr %elem_ptr218, align 8
  %elem_ptr219 = getelementptr ptr, ptr %arr_data187, i64 31
  store ptr @str.68, ptr %elem_ptr219, align 8
  %elem_ptr220 = getelementptr ptr, ptr %arr_data187, i64 32
  store ptr @str.69, ptr %elem_ptr220, align 8
  %elem_ptr221 = getelementptr ptr, ptr %arr_data187, i64 33
  store ptr @str.70, ptr %elem_ptr221, align 8
  %elem_ptr222 = getelementptr ptr, ptr %arr_data187, i64 34
  store ptr @str.71, ptr %elem_ptr222, align 8
  %elem_ptr223 = getelementptr ptr, ptr %arr_data187, i64 35
  store ptr @str.72, ptr %elem_ptr223, align 8
  %elem_ptr224 = getelementptr ptr, ptr %arr_data187, i64 36
  store ptr @str.73, ptr %elem_ptr224, align 8
  %arr_header225 = call ptr @malloc(i64 24)
  %arr_data226 = call ptr @malloc(i64 296)
  %123 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header225, i32 0, i32 0
  %124 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header225, i32 0, i32 1
  %125 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header225, i32 0, i32 2
  store i64 37, ptr %123, align 8
  store i64 37, ptr %124, align 8
  store ptr %arr_data226, ptr %125, align 8
  %elem_ptr227 = getelementptr i64, ptr %arr_data226, i64 0
  store i64 4, ptr %elem_ptr227, align 8
  %elem_ptr228 = getelementptr i64, ptr %arr_data226, i64 1
  store i64 2, ptr %elem_ptr228, align 8
  %elem_ptr229 = getelementptr i64, ptr %arr_data226, i64 2
  store i64 2, ptr %elem_ptr229, align 8
  %elem_ptr230 = getelementptr i64, ptr %arr_data226, i64 3
  store i64 2, ptr %elem_ptr230, align 8
  %elem_ptr231 = getelementptr i64, ptr %arr_data226, i64 4
  store i64 2, ptr %elem_ptr231, align 8
  %elem_ptr232 = getelementptr i64, ptr %arr_data226, i64 5
  store i64 2, ptr %elem_ptr232, align 8
  %elem_ptr233 = getelementptr i64, ptr %arr_data226, i64 6
  store i64 2, ptr %elem_ptr233, align 8
  %elem_ptr234 = getelementptr i64, ptr %arr_data226, i64 7
  store i64 2, ptr %elem_ptr234, align 8
  %elem_ptr235 = getelementptr i64, ptr %arr_data226, i64 8
  store i64 2, ptr %elem_ptr235, align 8
  %elem_ptr236 = getelementptr i64, ptr %arr_data226, i64 9
  store i64 2, ptr %elem_ptr236, align 8
  %elem_ptr237 = getelementptr i64, ptr %arr_data226, i64 10
  store i64 2, ptr %elem_ptr237, align 8
  %elem_ptr238 = getelementptr i64, ptr %arr_data226, i64 11
  store i64 2, ptr %elem_ptr238, align 8
  %elem_ptr239 = getelementptr i64, ptr %arr_data226, i64 12
  store i64 2, ptr %elem_ptr239, align 8
  %elem_ptr240 = getelementptr i64, ptr %arr_data226, i64 13
  store i64 2, ptr %elem_ptr240, align 8
  %elem_ptr241 = getelementptr i64, ptr %arr_data226, i64 14
  store i64 2, ptr %elem_ptr241, align 8
  %elem_ptr242 = getelementptr i64, ptr %arr_data226, i64 15
  store i64 2, ptr %elem_ptr242, align 8
  %elem_ptr243 = getelementptr i64, ptr %arr_data226, i64 16
  store i64 2, ptr %elem_ptr243, align 8
  %elem_ptr244 = getelementptr i64, ptr %arr_data226, i64 17
  store i64 2, ptr %elem_ptr244, align 8
  %elem_ptr245 = getelementptr i64, ptr %arr_data226, i64 18
  store i64 2, ptr %elem_ptr245, align 8
  %elem_ptr246 = getelementptr i64, ptr %arr_data226, i64 19
  store i64 2, ptr %elem_ptr246, align 8
  %elem_ptr247 = getelementptr i64, ptr %arr_data226, i64 20
  store i64 1, ptr %elem_ptr247, align 8
  %elem_ptr248 = getelementptr i64, ptr %arr_data226, i64 21
  store i64 3, ptr %elem_ptr248, align 8
  %elem_ptr249 = getelementptr i64, ptr %arr_data226, i64 22
  store i64 3, ptr %elem_ptr249, align 8
  %elem_ptr250 = getelementptr i64, ptr %arr_data226, i64 23
  store i64 3, ptr %elem_ptr250, align 8
  %elem_ptr251 = getelementptr i64, ptr %arr_data226, i64 24
  store i64 3, ptr %elem_ptr251, align 8
  %elem_ptr252 = getelementptr i64, ptr %arr_data226, i64 25
  store i64 3, ptr %elem_ptr252, align 8
  %elem_ptr253 = getelementptr i64, ptr %arr_data226, i64 26
  store i64 3, ptr %elem_ptr253, align 8
  %elem_ptr254 = getelementptr i64, ptr %arr_data226, i64 27
  store i64 3, ptr %elem_ptr254, align 8
  %elem_ptr255 = getelementptr i64, ptr %arr_data226, i64 28
  store i64 3, ptr %elem_ptr255, align 8
  %elem_ptr256 = getelementptr i64, ptr %arr_data226, i64 29
  store i64 3, ptr %elem_ptr256, align 8
  %elem_ptr257 = getelementptr i64, ptr %arr_data226, i64 30
  store i64 3, ptr %elem_ptr257, align 8
  %elem_ptr258 = getelementptr i64, ptr %arr_data226, i64 31
  store i64 3, ptr %elem_ptr258, align 8
  %elem_ptr259 = getelementptr i64, ptr %arr_data226, i64 32
  store i64 3, ptr %elem_ptr259, align 8
  %elem_ptr260 = getelementptr i64, ptr %arr_data226, i64 33
  store i64 3, ptr %elem_ptr260, align 8
  %elem_ptr261 = getelementptr i64, ptr %arr_data226, i64 34
  store i64 3, ptr %elem_ptr261, align 8
  %elem_ptr262 = getelementptr i64, ptr %arr_data226, i64 35
  store i64 3, ptr %elem_ptr262, align 8
  %elem_ptr263 = getelementptr i64, ptr %arr_data226, i64 36
  store i64 3, ptr %elem_ptr263, align 8
  %df_instance = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %126 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 0
  store ptr %arr_header186, ptr %126, align 8
  %127 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 1
  store ptr %data_header, ptr %127, align 8
  %128 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 2
  store ptr %arr_header225, ptr %128, align 8
  %129 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 3
  store i64 0, ptr %129, align 4
  store ptr %df_instance, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_load = load i64, ptr @__where_i, align 8
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %rowCount_ptr = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %__where_src_load, i32 0, i32 3
  %rowCount = load i64, ptr %rowCount_ptr, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %rowCount
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_i_load264 = load i64, ptr @__where_i, align 8
  %__where_src_load265 = load ptr, ptr @__where_src, align 8
  %df_data_ptr = getelementptr inbounds nuw %dataframe, ptr %__where_src_load265, i32 0, i32 1
  %data_header266 = load ptr, ptr %df_data_ptr, align 8
  %130 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header266, i32 0, i32 2
  %131 = load ptr, ptr %130, align 8
  %132 = getelementptr ptr, ptr %131, i64 1
  %133 = load ptr, ptr %132, align 8
  %134 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %133, i32 0, i32 0
  %135 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %133, i32 0, i32 2
  %136 = load i64, ptr %134, align 4
  %137 = load ptr, ptr %135, align 8
  %138 = icmp ult i64 %__where_i_load264, %136
  br i1 %138, label %arr_ok, label %arr_err

for.step:                                         ; preds = %ifcont
  %x_load = load i64, ptr @__where_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__where_result_load1175 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__where_result_load1175, ptr %data_ptr, align 8
  ret ptr %runtime_obj

arr_ok:                                           ; preds = %for.body
  %139 = getelementptr double, ptr %137, i64 %__where_i_load264
  %140 = load double, ptr %139, align 8
  br label %arr_merge

arr_err:                                          ; preds = %for.body
  %141 = call i32 (ptr, ...) @printf(ptr @0)
  br label %arr_merge

arr_merge:                                        ; preds = %arr_ok, %arr_err
  %arr_val = phi double [ 0.000000e+00, %arr_err ], [ %140, %arr_ok ]
  %fcmp_tmp = fcmp ogt double %arr_val, -1.800000e+01
  br i1 %fcmp_tmp, label %then, label %else

then:                                             ; preds = %arr_merge
  %record_mem = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr null, i32 1) to i32))
  %__where_i_load267 = load i64, ptr @__where_i, align 8
  %__where_src_load268 = load ptr, ptr @__where_src, align 8
  %df_data_ptr269 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load268, i32 0, i32 1
  %data_header270 = load ptr, ptr %df_data_ptr269, align 8
  %142 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header270, i32 0, i32 2
  %143 = load ptr, ptr %142, align 8
  %144 = getelementptr ptr, ptr %143, i64 0
  %145 = load ptr, ptr %144, align 8
  %146 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %145, i32 0, i32 0
  %147 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %145, i32 0, i32 2
  %148 = load i64, ptr %146, align 4
  %149 = load ptr, ptr %147, align 8
  %150 = icmp ult i64 %__where_i_load267, %148
  br i1 %150, label %arr_ok271, label %arr_err272

else:                                             ; preds = %arr_merge
  br label %ifcont

ifcont:                                           ; preds = %else, %store_element1167
  br label %for.step

arr_ok271:                                        ; preds = %then
  %151 = getelementptr ptr, ptr %149, i64 %__where_i_load267
  %152 = load ptr, ptr %151, align 8
  br label %arr_merge273

arr_err272:                                       ; preds = %then
  %153 = call i32 (ptr, ...) @printf(ptr @1)
  br label %arr_merge273

arr_merge273:                                     ; preds = %arr_ok271, %arr_err272
  %arr_val274 = phi ptr [ null, %arr_err272 ], [ %152, %arr_ok271 ]
  %ptr_date = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 0
  store ptr %arr_val274, ptr %ptr_date, align 8
  %__where_i_load275 = load i64, ptr @__where_i, align 8
  %__where_src_load276 = load ptr, ptr @__where_src, align 8
  %df_data_ptr277 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load276, i32 0, i32 1
  %data_header278 = load ptr, ptr %df_data_ptr277, align 8
  %154 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header278, i32 0, i32 2
  %155 = load ptr, ptr %154, align 8
  %156 = getelementptr ptr, ptr %155, i64 1
  %157 = load ptr, ptr %156, align 8
  %158 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %157, i32 0, i32 0
  %159 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %157, i32 0, i32 2
  %160 = load i64, ptr %158, align 4
  %161 = load ptr, ptr %159, align 8
  %162 = icmp ult i64 %__where_i_load275, %160
  br i1 %162, label %arr_ok279, label %arr_err280

arr_ok279:                                        ; preds = %arr_merge273
  %163 = getelementptr double, ptr %161, i64 %__where_i_load275
  %164 = load double, ptr %163, align 8
  br label %arr_merge281

arr_err280:                                       ; preds = %arr_merge273
  %165 = call i32 (ptr, ...) @printf(ptr @2)
  br label %arr_merge281

arr_merge281:                                     ; preds = %arr_ok279, %arr_err280
  %arr_val282 = phi double [ 0.000000e+00, %arr_err280 ], [ %164, %arr_ok279 ]
  %ptr_latitude = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 1
  store double %arr_val282, ptr %ptr_latitude, align 8
  %__where_i_load283 = load i64, ptr @__where_i, align 8
  %__where_src_load284 = load ptr, ptr @__where_src, align 8
  %df_data_ptr285 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load284, i32 0, i32 1
  %data_header286 = load ptr, ptr %df_data_ptr285, align 8
  %166 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header286, i32 0, i32 2
  %167 = load ptr, ptr %166, align 8
  %168 = getelementptr ptr, ptr %167, i64 2
  %169 = load ptr, ptr %168, align 8
  %170 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %169, i32 0, i32 0
  %171 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %169, i32 0, i32 2
  %172 = load i64, ptr %170, align 4
  %173 = load ptr, ptr %171, align 8
  %174 = icmp ult i64 %__where_i_load283, %172
  br i1 %174, label %arr_ok287, label %arr_err288

arr_ok287:                                        ; preds = %arr_merge281
  %175 = getelementptr double, ptr %173, i64 %__where_i_load283
  %176 = load double, ptr %175, align 8
  br label %arr_merge289

arr_err288:                                       ; preds = %arr_merge281
  %177 = call i32 (ptr, ...) @printf(ptr @3)
  br label %arr_merge289

arr_merge289:                                     ; preds = %arr_ok287, %arr_err288
  %arr_val290 = phi double [ 0.000000e+00, %arr_err288 ], [ %176, %arr_ok287 ]
  %ptr_longitude = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 2
  store double %arr_val290, ptr %ptr_longitude, align 8
  %__where_i_load291 = load i64, ptr @__where_i, align 8
  %__where_src_load292 = load ptr, ptr @__where_src, align 8
  %df_data_ptr293 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load292, i32 0, i32 1
  %data_header294 = load ptr, ptr %df_data_ptr293, align 8
  %178 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header294, i32 0, i32 2
  %179 = load ptr, ptr %178, align 8
  %180 = getelementptr ptr, ptr %179, i64 3
  %181 = load ptr, ptr %180, align 8
  %182 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %181, i32 0, i32 0
  %183 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %181, i32 0, i32 2
  %184 = load i64, ptr %182, align 4
  %185 = load ptr, ptr %183, align 8
  %186 = icmp ult i64 %__where_i_load291, %184
  br i1 %186, label %arr_ok295, label %arr_err296

arr_ok295:                                        ; preds = %arr_merge289
  %187 = getelementptr double, ptr %185, i64 %__where_i_load291
  %188 = load double, ptr %187, align 8
  br label %arr_merge297

arr_err296:                                       ; preds = %arr_merge289
  %189 = call i32 (ptr, ...) @printf(ptr @4)
  br label %arr_merge297

arr_merge297:                                     ; preds = %arr_ok295, %arr_err296
  %arr_val298 = phi double [ 0.000000e+00, %arr_err296 ], [ %188, %arr_ok295 ]
  %ptr_wind-speed-min = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 3
  store double %arr_val298, ptr %ptr_wind-speed-min, align 8
  %__where_i_load299 = load i64, ptr @__where_i, align 8
  %__where_src_load300 = load ptr, ptr @__where_src, align 8
  %df_data_ptr301 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load300, i32 0, i32 1
  %data_header302 = load ptr, ptr %df_data_ptr301, align 8
  %190 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header302, i32 0, i32 2
  %191 = load ptr, ptr %190, align 8
  %192 = getelementptr ptr, ptr %191, i64 4
  %193 = load ptr, ptr %192, align 8
  %194 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %193, i32 0, i32 0
  %195 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %193, i32 0, i32 2
  %196 = load i64, ptr %194, align 4
  %197 = load ptr, ptr %195, align 8
  %198 = icmp ult i64 %__where_i_load299, %196
  br i1 %198, label %arr_ok303, label %arr_err304

arr_ok303:                                        ; preds = %arr_merge297
  %199 = getelementptr double, ptr %197, i64 %__where_i_load299
  %200 = load double, ptr %199, align 8
  br label %arr_merge305

arr_err304:                                       ; preds = %arr_merge297
  %201 = call i32 (ptr, ...) @printf(ptr @5)
  br label %arr_merge305

arr_merge305:                                     ; preds = %arr_ok303, %arr_err304
  %arr_val306 = phi double [ 0.000000e+00, %arr_err304 ], [ %200, %arr_ok303 ]
  %ptr_wind-speed-max = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 4
  store double %arr_val306, ptr %ptr_wind-speed-max, align 8
  %__where_i_load307 = load i64, ptr @__where_i, align 8
  %__where_src_load308 = load ptr, ptr @__where_src, align 8
  %df_data_ptr309 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load308, i32 0, i32 1
  %data_header310 = load ptr, ptr %df_data_ptr309, align 8
  %202 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header310, i32 0, i32 2
  %203 = load ptr, ptr %202, align 8
  %204 = getelementptr ptr, ptr %203, i64 5
  %205 = load ptr, ptr %204, align 8
  %206 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %205, i32 0, i32 0
  %207 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %205, i32 0, i32 2
  %208 = load i64, ptr %206, align 4
  %209 = load ptr, ptr %207, align 8
  %210 = icmp ult i64 %__where_i_load307, %208
  br i1 %210, label %arr_ok311, label %arr_err312

arr_ok311:                                        ; preds = %arr_merge305
  %211 = getelementptr double, ptr %209, i64 %__where_i_load307
  %212 = load double, ptr %211, align 8
  br label %arr_merge313

arr_err312:                                       ; preds = %arr_merge305
  %213 = call i32 (ptr, ...) @printf(ptr @6)
  br label %arr_merge313

arr_merge313:                                     ; preds = %arr_ok311, %arr_err312
  %arr_val314 = phi double [ 0.000000e+00, %arr_err312 ], [ %212, %arr_ok311 ]
  %ptr_wind-speed-mean = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 5
  store double %arr_val314, ptr %ptr_wind-speed-mean, align 8
  %__where_i_load315 = load i64, ptr @__where_i, align 8
  %__where_src_load316 = load ptr, ptr @__where_src, align 8
  %df_data_ptr317 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load316, i32 0, i32 1
  %data_header318 = load ptr, ptr %df_data_ptr317, align 8
  %214 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header318, i32 0, i32 2
  %215 = load ptr, ptr %214, align 8
  %216 = getelementptr ptr, ptr %215, i64 6
  %217 = load ptr, ptr %216, align 8
  %218 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %217, i32 0, i32 0
  %219 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %217, i32 0, i32 2
  %220 = load i64, ptr %218, align 4
  %221 = load ptr, ptr %219, align 8
  %222 = icmp ult i64 %__where_i_load315, %220
  br i1 %222, label %arr_ok319, label %arr_err320

arr_ok319:                                        ; preds = %arr_merge313
  %223 = getelementptr double, ptr %221, i64 %__where_i_load315
  %224 = load double, ptr %223, align 8
  br label %arr_merge321

arr_err320:                                       ; preds = %arr_merge313
  %225 = call i32 (ptr, ...) @printf(ptr @7)
  br label %arr_merge321

arr_merge321:                                     ; preds = %arr_ok319, %arr_err320
  %arr_val322 = phi double [ 0.000000e+00, %arr_err320 ], [ %224, %arr_ok319 ]
  %ptr_wind-direction-min = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 6
  store double %arr_val322, ptr %ptr_wind-direction-min, align 8
  %__where_i_load323 = load i64, ptr @__where_i, align 8
  %__where_src_load324 = load ptr, ptr @__where_src, align 8
  %df_data_ptr325 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load324, i32 0, i32 1
  %data_header326 = load ptr, ptr %df_data_ptr325, align 8
  %226 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header326, i32 0, i32 2
  %227 = load ptr, ptr %226, align 8
  %228 = getelementptr ptr, ptr %227, i64 7
  %229 = load ptr, ptr %228, align 8
  %230 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %229, i32 0, i32 0
  %231 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %229, i32 0, i32 2
  %232 = load i64, ptr %230, align 4
  %233 = load ptr, ptr %231, align 8
  %234 = icmp ult i64 %__where_i_load323, %232
  br i1 %234, label %arr_ok327, label %arr_err328

arr_ok327:                                        ; preds = %arr_merge321
  %235 = getelementptr double, ptr %233, i64 %__where_i_load323
  %236 = load double, ptr %235, align 8
  br label %arr_merge329

arr_err328:                                       ; preds = %arr_merge321
  %237 = call i32 (ptr, ...) @printf(ptr @8)
  br label %arr_merge329

arr_merge329:                                     ; preds = %arr_ok327, %arr_err328
  %arr_val330 = phi double [ 0.000000e+00, %arr_err328 ], [ %236, %arr_ok327 ]
  %ptr_wind-direction-max = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 7
  store double %arr_val330, ptr %ptr_wind-direction-max, align 8
  %__where_i_load331 = load i64, ptr @__where_i, align 8
  %__where_src_load332 = load ptr, ptr @__where_src, align 8
  %df_data_ptr333 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load332, i32 0, i32 1
  %data_header334 = load ptr, ptr %df_data_ptr333, align 8
  %238 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header334, i32 0, i32 2
  %239 = load ptr, ptr %238, align 8
  %240 = getelementptr ptr, ptr %239, i64 8
  %241 = load ptr, ptr %240, align 8
  %242 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %241, i32 0, i32 0
  %243 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %241, i32 0, i32 2
  %244 = load i64, ptr %242, align 4
  %245 = load ptr, ptr %243, align 8
  %246 = icmp ult i64 %__where_i_load331, %244
  br i1 %246, label %arr_ok335, label %arr_err336

arr_ok335:                                        ; preds = %arr_merge329
  %247 = getelementptr double, ptr %245, i64 %__where_i_load331
  %248 = load double, ptr %247, align 8
  br label %arr_merge337

arr_err336:                                       ; preds = %arr_merge329
  %249 = call i32 (ptr, ...) @printf(ptr @9)
  br label %arr_merge337

arr_merge337:                                     ; preds = %arr_ok335, %arr_err336
  %arr_val338 = phi double [ 0.000000e+00, %arr_err336 ], [ %248, %arr_ok335 ]
  %ptr_wind-direction-mean = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 8
  store double %arr_val338, ptr %ptr_wind-direction-mean, align 8
  %__where_i_load339 = load i64, ptr @__where_i, align 8
  %__where_src_load340 = load ptr, ptr @__where_src, align 8
  %df_data_ptr341 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load340, i32 0, i32 1
  %data_header342 = load ptr, ptr %df_data_ptr341, align 8
  %250 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header342, i32 0, i32 2
  %251 = load ptr, ptr %250, align 8
  %252 = getelementptr ptr, ptr %251, i64 9
  %253 = load ptr, ptr %252, align 8
  %254 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %253, i32 0, i32 0
  %255 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %253, i32 0, i32 2
  %256 = load i64, ptr %254, align 4
  %257 = load ptr, ptr %255, align 8
  %258 = icmp ult i64 %__where_i_load339, %256
  br i1 %258, label %arr_ok343, label %arr_err344

arr_ok343:                                        ; preds = %arr_merge337
  %259 = getelementptr double, ptr %257, i64 %__where_i_load339
  %260 = load double, ptr %259, align 8
  br label %arr_merge345

arr_err344:                                       ; preds = %arr_merge337
  %261 = call i32 (ptr, ...) @printf(ptr @10)
  br label %arr_merge345

arr_merge345:                                     ; preds = %arr_ok343, %arr_err344
  %arr_val346 = phi double [ 0.000000e+00, %arr_err344 ], [ %260, %arr_ok343 ]
  %ptr_surface-air-temperature-min = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 9
  store double %arr_val346, ptr %ptr_surface-air-temperature-min, align 8
  %__where_i_load347 = load i64, ptr @__where_i, align 8
  %__where_src_load348 = load ptr, ptr @__where_src, align 8
  %df_data_ptr349 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load348, i32 0, i32 1
  %data_header350 = load ptr, ptr %df_data_ptr349, align 8
  %262 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header350, i32 0, i32 2
  %263 = load ptr, ptr %262, align 8
  %264 = getelementptr ptr, ptr %263, i64 10
  %265 = load ptr, ptr %264, align 8
  %266 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %265, i32 0, i32 0
  %267 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %265, i32 0, i32 2
  %268 = load i64, ptr %266, align 4
  %269 = load ptr, ptr %267, align 8
  %270 = icmp ult i64 %__where_i_load347, %268
  br i1 %270, label %arr_ok351, label %arr_err352

arr_ok351:                                        ; preds = %arr_merge345
  %271 = getelementptr double, ptr %269, i64 %__where_i_load347
  %272 = load double, ptr %271, align 8
  br label %arr_merge353

arr_err352:                                       ; preds = %arr_merge345
  %273 = call i32 (ptr, ...) @printf(ptr @11)
  br label %arr_merge353

arr_merge353:                                     ; preds = %arr_ok351, %arr_err352
  %arr_val354 = phi double [ 0.000000e+00, %arr_err352 ], [ %272, %arr_ok351 ]
  %ptr_surface-air-temperature-max = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 10
  store double %arr_val354, ptr %ptr_surface-air-temperature-max, align 8
  %__where_i_load355 = load i64, ptr @__where_i, align 8
  %__where_src_load356 = load ptr, ptr @__where_src, align 8
  %df_data_ptr357 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load356, i32 0, i32 1
  %data_header358 = load ptr, ptr %df_data_ptr357, align 8
  %274 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header358, i32 0, i32 2
  %275 = load ptr, ptr %274, align 8
  %276 = getelementptr ptr, ptr %275, i64 11
  %277 = load ptr, ptr %276, align 8
  %278 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %277, i32 0, i32 0
  %279 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %277, i32 0, i32 2
  %280 = load i64, ptr %278, align 4
  %281 = load ptr, ptr %279, align 8
  %282 = icmp ult i64 %__where_i_load355, %280
  br i1 %282, label %arr_ok359, label %arr_err360

arr_ok359:                                        ; preds = %arr_merge353
  %283 = getelementptr double, ptr %281, i64 %__where_i_load355
  %284 = load double, ptr %283, align 8
  br label %arr_merge361

arr_err360:                                       ; preds = %arr_merge353
  %285 = call i32 (ptr, ...) @printf(ptr @12)
  br label %arr_merge361

arr_merge361:                                     ; preds = %arr_ok359, %arr_err360
  %arr_val362 = phi double [ 0.000000e+00, %arr_err360 ], [ %284, %arr_ok359 ]
  %ptr_surface-air-temperature-mean = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 11
  store double %arr_val362, ptr %ptr_surface-air-temperature-mean, align 8
  %__where_i_load363 = load i64, ptr @__where_i, align 8
  %__where_src_load364 = load ptr, ptr @__where_src, align 8
  %df_data_ptr365 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load364, i32 0, i32 1
  %data_header366 = load ptr, ptr %df_data_ptr365, align 8
  %286 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header366, i32 0, i32 2
  %287 = load ptr, ptr %286, align 8
  %288 = getelementptr ptr, ptr %287, i64 12
  %289 = load ptr, ptr %288, align 8
  %290 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %289, i32 0, i32 0
  %291 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %289, i32 0, i32 2
  %292 = load i64, ptr %290, align 4
  %293 = load ptr, ptr %291, align 8
  %294 = icmp ult i64 %__where_i_load363, %292
  br i1 %294, label %arr_ok367, label %arr_err368

arr_ok367:                                        ; preds = %arr_merge361
  %295 = getelementptr double, ptr %293, i64 %__where_i_load363
  %296 = load double, ptr %295, align 8
  br label %arr_merge369

arr_err368:                                       ; preds = %arr_merge361
  %297 = call i32 (ptr, ...) @printf(ptr @13)
  br label %arr_merge369

arr_merge369:                                     ; preds = %arr_ok367, %arr_err368
  %arr_val370 = phi double [ 0.000000e+00, %arr_err368 ], [ %296, %arr_ok367 ]
  %ptr_total-rainfall-sum = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 12
  store double %arr_val370, ptr %ptr_total-rainfall-sum, align 8
  %__where_i_load371 = load i64, ptr @__where_i, align 8
  %__where_src_load372 = load ptr, ptr @__where_src, align 8
  %df_data_ptr373 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load372, i32 0, i32 1
  %data_header374 = load ptr, ptr %df_data_ptr373, align 8
  %298 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header374, i32 0, i32 2
  %299 = load ptr, ptr %298, align 8
  %300 = getelementptr ptr, ptr %299, i64 13
  %301 = load ptr, ptr %300, align 8
  %302 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %301, i32 0, i32 0
  %303 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %301, i32 0, i32 2
  %304 = load i64, ptr %302, align 4
  %305 = load ptr, ptr %303, align 8
  %306 = icmp ult i64 %__where_i_load371, %304
  br i1 %306, label %arr_ok375, label %arr_err376

arr_ok375:                                        ; preds = %arr_merge369
  %307 = getelementptr double, ptr %305, i64 %__where_i_load371
  %308 = load double, ptr %307, align 8
  br label %arr_merge377

arr_err376:                                       ; preds = %arr_merge369
  %309 = call i32 (ptr, ...) @printf(ptr @14)
  br label %arr_merge377

arr_merge377:                                     ; preds = %arr_ok375, %arr_err376
  %arr_val378 = phi double [ 0.000000e+00, %arr_err376 ], [ %308, %arr_ok375 ]
  %ptr_surface-humidity-min = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 13
  store double %arr_val378, ptr %ptr_surface-humidity-min, align 8
  %__where_i_load379 = load i64, ptr @__where_i, align 8
  %__where_src_load380 = load ptr, ptr @__where_src, align 8
  %df_data_ptr381 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load380, i32 0, i32 1
  %data_header382 = load ptr, ptr %df_data_ptr381, align 8
  %310 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header382, i32 0, i32 2
  %311 = load ptr, ptr %310, align 8
  %312 = getelementptr ptr, ptr %311, i64 14
  %313 = load ptr, ptr %312, align 8
  %314 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %313, i32 0, i32 0
  %315 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %313, i32 0, i32 2
  %316 = load i64, ptr %314, align 4
  %317 = load ptr, ptr %315, align 8
  %318 = icmp ult i64 %__where_i_load379, %316
  br i1 %318, label %arr_ok383, label %arr_err384

arr_ok383:                                        ; preds = %arr_merge377
  %319 = getelementptr double, ptr %317, i64 %__where_i_load379
  %320 = load double, ptr %319, align 8
  br label %arr_merge385

arr_err384:                                       ; preds = %arr_merge377
  %321 = call i32 (ptr, ...) @printf(ptr @15)
  br label %arr_merge385

arr_merge385:                                     ; preds = %arr_ok383, %arr_err384
  %arr_val386 = phi double [ 0.000000e+00, %arr_err384 ], [ %320, %arr_ok383 ]
  %ptr_surface-humidity-max = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 14
  store double %arr_val386, ptr %ptr_surface-humidity-max, align 8
  %__where_i_load387 = load i64, ptr @__where_i, align 8
  %__where_src_load388 = load ptr, ptr @__where_src, align 8
  %df_data_ptr389 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load388, i32 0, i32 1
  %data_header390 = load ptr, ptr %df_data_ptr389, align 8
  %322 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header390, i32 0, i32 2
  %323 = load ptr, ptr %322, align 8
  %324 = getelementptr ptr, ptr %323, i64 15
  %325 = load ptr, ptr %324, align 8
  %326 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %325, i32 0, i32 0
  %327 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %325, i32 0, i32 2
  %328 = load i64, ptr %326, align 4
  %329 = load ptr, ptr %327, align 8
  %330 = icmp ult i64 %__where_i_load387, %328
  br i1 %330, label %arr_ok391, label %arr_err392

arr_ok391:                                        ; preds = %arr_merge385
  %331 = getelementptr double, ptr %329, i64 %__where_i_load387
  %332 = load double, ptr %331, align 8
  br label %arr_merge393

arr_err392:                                       ; preds = %arr_merge385
  %333 = call i32 (ptr, ...) @printf(ptr @16)
  br label %arr_merge393

arr_merge393:                                     ; preds = %arr_ok391, %arr_err392
  %arr_val394 = phi double [ 0.000000e+00, %arr_err392 ], [ %332, %arr_ok391 ]
  %ptr_surface-humidity-mean = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 15
  store double %arr_val394, ptr %ptr_surface-humidity-mean, align 8
  %__where_i_load395 = load i64, ptr @__where_i, align 8
  %__where_src_load396 = load ptr, ptr @__where_src, align 8
  %df_data_ptr397 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load396, i32 0, i32 1
  %data_header398 = load ptr, ptr %df_data_ptr397, align 8
  %334 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header398, i32 0, i32 2
  %335 = load ptr, ptr %334, align 8
  %336 = getelementptr ptr, ptr %335, i64 16
  %337 = load ptr, ptr %336, align 8
  %338 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %337, i32 0, i32 0
  %339 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %337, i32 0, i32 2
  %340 = load i64, ptr %338, align 4
  %341 = load ptr, ptr %339, align 8
  %342 = icmp ult i64 %__where_i_load395, %340
  br i1 %342, label %arr_ok399, label %arr_err400

arr_ok399:                                        ; preds = %arr_merge393
  %343 = getelementptr double, ptr %341, i64 %__where_i_load395
  %344 = load double, ptr %343, align 8
  br label %arr_merge401

arr_err400:                                       ; preds = %arr_merge393
  %345 = call i32 (ptr, ...) @printf(ptr @17)
  br label %arr_merge401

arr_merge401:                                     ; preds = %arr_ok399, %arr_err400
  %arr_val402 = phi double [ 0.000000e+00, %arr_err400 ], [ %344, %arr_ok399 ]
  %ptr_ndvi = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 16
  store double %arr_val402, ptr %ptr_ndvi, align 8
  %__where_i_load403 = load i64, ptr @__where_i, align 8
  %__where_src_load404 = load ptr, ptr @__where_src, align 8
  %df_data_ptr405 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load404, i32 0, i32 1
  %data_header406 = load ptr, ptr %df_data_ptr405, align 8
  %346 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header406, i32 0, i32 2
  %347 = load ptr, ptr %346, align 8
  %348 = getelementptr ptr, ptr %347, i64 17
  %349 = load ptr, ptr %348, align 8
  %350 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %349, i32 0, i32 0
  %351 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %349, i32 0, i32 2
  %352 = load i64, ptr %350, align 4
  %353 = load ptr, ptr %351, align 8
  %354 = icmp ult i64 %__where_i_load403, %352
  br i1 %354, label %arr_ok407, label %arr_err408

arr_ok407:                                        ; preds = %arr_merge401
  %355 = getelementptr double, ptr %353, i64 %__where_i_load403
  %356 = load double, ptr %355, align 8
  br label %arr_merge409

arr_err408:                                       ; preds = %arr_merge401
  %357 = call i32 (ptr, ...) @printf(ptr @18)
  br label %arr_merge409

arr_merge409:                                     ; preds = %arr_ok407, %arr_err408
  %arr_val410 = phi double [ 0.000000e+00, %arr_err408 ], [ %356, %arr_ok407 ]
  %ptr_elevation = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 17
  store double %arr_val410, ptr %ptr_elevation, align 8
  %__where_i_load411 = load i64, ptr @__where_i, align 8
  %__where_src_load412 = load ptr, ptr @__where_src, align 8
  %df_data_ptr413 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load412, i32 0, i32 1
  %data_header414 = load ptr, ptr %df_data_ptr413, align 8
  %358 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header414, i32 0, i32 2
  %359 = load ptr, ptr %358, align 8
  %360 = getelementptr ptr, ptr %359, i64 18
  %361 = load ptr, ptr %360, align 8
  %362 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %361, i32 0, i32 0
  %363 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %361, i32 0, i32 2
  %364 = load i64, ptr %362, align 4
  %365 = load ptr, ptr %363, align 8
  %366 = icmp ult i64 %__where_i_load411, %364
  br i1 %366, label %arr_ok415, label %arr_err416

arr_ok415:                                        ; preds = %arr_merge409
  %367 = getelementptr double, ptr %365, i64 %__where_i_load411
  %368 = load double, ptr %367, align 8
  br label %arr_merge417

arr_err416:                                       ; preds = %arr_merge409
  %369 = call i32 (ptr, ...) @printf(ptr @19)
  br label %arr_merge417

arr_merge417:                                     ; preds = %arr_ok415, %arr_err416
  %arr_val418 = phi double [ 0.000000e+00, %arr_err416 ], [ %368, %arr_ok415 ]
  %ptr_slope = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 18
  store double %arr_val418, ptr %ptr_slope, align 8
  %__where_i_load419 = load i64, ptr @__where_i, align 8
  %__where_src_load420 = load ptr, ptr @__where_src, align 8
  %df_data_ptr421 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load420, i32 0, i32 1
  %data_header422 = load ptr, ptr %df_data_ptr421, align 8
  %370 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header422, i32 0, i32 2
  %371 = load ptr, ptr %370, align 8
  %372 = getelementptr ptr, ptr %371, i64 19
  %373 = load ptr, ptr %372, align 8
  %374 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %373, i32 0, i32 0
  %375 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %373, i32 0, i32 2
  %376 = load i64, ptr %374, align 4
  %377 = load ptr, ptr %375, align 8
  %378 = icmp ult i64 %__where_i_load419, %376
  br i1 %378, label %arr_ok423, label %arr_err424

arr_ok423:                                        ; preds = %arr_merge417
  %379 = getelementptr double, ptr %377, i64 %__where_i_load419
  %380 = load double, ptr %379, align 8
  br label %arr_merge425

arr_err424:                                       ; preds = %arr_merge417
  %381 = call i32 (ptr, ...) @printf(ptr @20)
  br label %arr_merge425

arr_merge425:                                     ; preds = %arr_ok423, %arr_err424
  %arr_val426 = phi double [ 0.000000e+00, %arr_err424 ], [ %380, %arr_ok423 ]
  %ptr_aspect = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 19
  store double %arr_val426, ptr %ptr_aspect, align 8
  %__where_i_load427 = load i64, ptr @__where_i, align 8
  %__where_src_load428 = load ptr, ptr @__where_src, align 8
  %df_data_ptr429 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load428, i32 0, i32 1
  %data_header430 = load ptr, ptr %df_data_ptr429, align 8
  %382 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header430, i32 0, i32 2
  %383 = load ptr, ptr %382, align 8
  %384 = getelementptr ptr, ptr %383, i64 20
  %385 = load ptr, ptr %384, align 8
  %386 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %385, i32 0, i32 0
  %387 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %385, i32 0, i32 2
  %388 = load i64, ptr %386, align 4
  %389 = load ptr, ptr %387, align 8
  %390 = icmp ult i64 %__where_i_load427, %388
  br i1 %390, label %arr_ok431, label %arr_err432

arr_ok431:                                        ; preds = %arr_merge425
  %391 = getelementptr i64, ptr %389, i64 %__where_i_load427
  %392 = load i64, ptr %391, align 4
  br label %arr_merge433

arr_err432:                                       ; preds = %arr_merge425
  %393 = call i32 (ptr, ...) @printf(ptr @21)
  br label %arr_merge433

arr_merge433:                                     ; preds = %arr_ok431, %arr_err432
  %arr_val434 = phi i64 [ 0, %arr_err432 ], [ %392, %arr_ok431 ]
  %ptr_fire_label = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 20
  store i64 %arr_val434, ptr %ptr_fire_label, align 4
  %__where_i_load435 = load i64, ptr @__where_i, align 8
  %__where_src_load436 = load ptr, ptr @__where_src, align 8
  %df_data_ptr437 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load436, i32 0, i32 1
  %data_header438 = load ptr, ptr %df_data_ptr437, align 8
  %394 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header438, i32 0, i32 2
  %395 = load ptr, ptr %394, align 8
  %396 = getelementptr ptr, ptr %395, i64 21
  %397 = load ptr, ptr %396, align 8
  %398 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %397, i32 0, i32 0
  %399 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %397, i32 0, i32 2
  %400 = load i64, ptr %398, align 4
  %401 = load ptr, ptr %399, align 8
  %402 = icmp ult i64 %__where_i_load435, %400
  br i1 %402, label %arr_ok439, label %arr_err440

arr_ok439:                                        ; preds = %arr_merge433
  %403 = getelementptr i8, ptr %401, i64 %__where_i_load435
  %404 = load i8, ptr %403, align 1
  br label %arr_merge441

arr_err440:                                       ; preds = %arr_merge433
  %405 = call i32 (ptr, ...) @printf(ptr @22)
  br label %arr_merge441

arr_merge441:                                     ; preds = %arr_ok439, %arr_err440
  %arr_val442 = phi i8 [ 0, %arr_err440 ], [ %404, %arr_ok439 ]
  %ptr_land_cover_class_1 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 21
  store i8 %arr_val442, ptr %ptr_land_cover_class_1, align 1
  %__where_i_load443 = load i64, ptr @__where_i, align 8
  %__where_src_load444 = load ptr, ptr @__where_src, align 8
  %df_data_ptr445 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load444, i32 0, i32 1
  %data_header446 = load ptr, ptr %df_data_ptr445, align 8
  %406 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header446, i32 0, i32 2
  %407 = load ptr, ptr %406, align 8
  %408 = getelementptr ptr, ptr %407, i64 22
  %409 = load ptr, ptr %408, align 8
  %410 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %409, i32 0, i32 0
  %411 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %409, i32 0, i32 2
  %412 = load i64, ptr %410, align 4
  %413 = load ptr, ptr %411, align 8
  %414 = icmp ult i64 %__where_i_load443, %412
  br i1 %414, label %arr_ok447, label %arr_err448

arr_ok447:                                        ; preds = %arr_merge441
  %415 = getelementptr i8, ptr %413, i64 %__where_i_load443
  %416 = load i8, ptr %415, align 1
  br label %arr_merge449

arr_err448:                                       ; preds = %arr_merge441
  %417 = call i32 (ptr, ...) @printf(ptr @23)
  br label %arr_merge449

arr_merge449:                                     ; preds = %arr_ok447, %arr_err448
  %arr_val450 = phi i8 [ 0, %arr_err448 ], [ %416, %arr_ok447 ]
  %ptr_land_cover_class_2 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 22
  store i8 %arr_val450, ptr %ptr_land_cover_class_2, align 1
  %__where_i_load451 = load i64, ptr @__where_i, align 8
  %__where_src_load452 = load ptr, ptr @__where_src, align 8
  %df_data_ptr453 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load452, i32 0, i32 1
  %data_header454 = load ptr, ptr %df_data_ptr453, align 8
  %418 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header454, i32 0, i32 2
  %419 = load ptr, ptr %418, align 8
  %420 = getelementptr ptr, ptr %419, i64 23
  %421 = load ptr, ptr %420, align 8
  %422 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %421, i32 0, i32 0
  %423 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %421, i32 0, i32 2
  %424 = load i64, ptr %422, align 4
  %425 = load ptr, ptr %423, align 8
  %426 = icmp ult i64 %__where_i_load451, %424
  br i1 %426, label %arr_ok455, label %arr_err456

arr_ok455:                                        ; preds = %arr_merge449
  %427 = getelementptr i8, ptr %425, i64 %__where_i_load451
  %428 = load i8, ptr %427, align 1
  br label %arr_merge457

arr_err456:                                       ; preds = %arr_merge449
  %429 = call i32 (ptr, ...) @printf(ptr @24)
  br label %arr_merge457

arr_merge457:                                     ; preds = %arr_ok455, %arr_err456
  %arr_val458 = phi i8 [ 0, %arr_err456 ], [ %428, %arr_ok455 ]
  %ptr_land_cover_class_4 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 23
  store i8 %arr_val458, ptr %ptr_land_cover_class_4, align 1
  %__where_i_load459 = load i64, ptr @__where_i, align 8
  %__where_src_load460 = load ptr, ptr @__where_src, align 8
  %df_data_ptr461 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load460, i32 0, i32 1
  %data_header462 = load ptr, ptr %df_data_ptr461, align 8
  %430 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header462, i32 0, i32 2
  %431 = load ptr, ptr %430, align 8
  %432 = getelementptr ptr, ptr %431, i64 24
  %433 = load ptr, ptr %432, align 8
  %434 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %433, i32 0, i32 0
  %435 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %433, i32 0, i32 2
  %436 = load i64, ptr %434, align 4
  %437 = load ptr, ptr %435, align 8
  %438 = icmp ult i64 %__where_i_load459, %436
  br i1 %438, label %arr_ok463, label %arr_err464

arr_ok463:                                        ; preds = %arr_merge457
  %439 = getelementptr i8, ptr %437, i64 %__where_i_load459
  %440 = load i8, ptr %439, align 1
  br label %arr_merge465

arr_err464:                                       ; preds = %arr_merge457
  %441 = call i32 (ptr, ...) @printf(ptr @25)
  br label %arr_merge465

arr_merge465:                                     ; preds = %arr_ok463, %arr_err464
  %arr_val466 = phi i8 [ 0, %arr_err464 ], [ %440, %arr_ok463 ]
  %ptr_land_cover_class_5 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 24
  store i8 %arr_val466, ptr %ptr_land_cover_class_5, align 1
  %__where_i_load467 = load i64, ptr @__where_i, align 8
  %__where_src_load468 = load ptr, ptr @__where_src, align 8
  %df_data_ptr469 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load468, i32 0, i32 1
  %data_header470 = load ptr, ptr %df_data_ptr469, align 8
  %442 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header470, i32 0, i32 2
  %443 = load ptr, ptr %442, align 8
  %444 = getelementptr ptr, ptr %443, i64 25
  %445 = load ptr, ptr %444, align 8
  %446 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %445, i32 0, i32 0
  %447 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %445, i32 0, i32 2
  %448 = load i64, ptr %446, align 4
  %449 = load ptr, ptr %447, align 8
  %450 = icmp ult i64 %__where_i_load467, %448
  br i1 %450, label %arr_ok471, label %arr_err472

arr_ok471:                                        ; preds = %arr_merge465
  %451 = getelementptr i8, ptr %449, i64 %__where_i_load467
  %452 = load i8, ptr %451, align 1
  br label %arr_merge473

arr_err472:                                       ; preds = %arr_merge465
  %453 = call i32 (ptr, ...) @printf(ptr @26)
  br label %arr_merge473

arr_merge473:                                     ; preds = %arr_ok471, %arr_err472
  %arr_val474 = phi i8 [ 0, %arr_err472 ], [ %452, %arr_ok471 ]
  %ptr_land_cover_class_6 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 25
  store i8 %arr_val474, ptr %ptr_land_cover_class_6, align 1
  %__where_i_load475 = load i64, ptr @__where_i, align 8
  %__where_src_load476 = load ptr, ptr @__where_src, align 8
  %df_data_ptr477 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load476, i32 0, i32 1
  %data_header478 = load ptr, ptr %df_data_ptr477, align 8
  %454 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header478, i32 0, i32 2
  %455 = load ptr, ptr %454, align 8
  %456 = getelementptr ptr, ptr %455, i64 26
  %457 = load ptr, ptr %456, align 8
  %458 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %457, i32 0, i32 0
  %459 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %457, i32 0, i32 2
  %460 = load i64, ptr %458, align 4
  %461 = load ptr, ptr %459, align 8
  %462 = icmp ult i64 %__where_i_load475, %460
  br i1 %462, label %arr_ok479, label %arr_err480

arr_ok479:                                        ; preds = %arr_merge473
  %463 = getelementptr i8, ptr %461, i64 %__where_i_load475
  %464 = load i8, ptr %463, align 1
  br label %arr_merge481

arr_err480:                                       ; preds = %arr_merge473
  %465 = call i32 (ptr, ...) @printf(ptr @27)
  br label %arr_merge481

arr_merge481:                                     ; preds = %arr_ok479, %arr_err480
  %arr_val482 = phi i8 [ 0, %arr_err480 ], [ %464, %arr_ok479 ]
  %ptr_land_cover_class_7 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 26
  store i8 %arr_val482, ptr %ptr_land_cover_class_7, align 1
  %__where_i_load483 = load i64, ptr @__where_i, align 8
  %__where_src_load484 = load ptr, ptr @__where_src, align 8
  %df_data_ptr485 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load484, i32 0, i32 1
  %data_header486 = load ptr, ptr %df_data_ptr485, align 8
  %466 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header486, i32 0, i32 2
  %467 = load ptr, ptr %466, align 8
  %468 = getelementptr ptr, ptr %467, i64 27
  %469 = load ptr, ptr %468, align 8
  %470 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %469, i32 0, i32 0
  %471 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %469, i32 0, i32 2
  %472 = load i64, ptr %470, align 4
  %473 = load ptr, ptr %471, align 8
  %474 = icmp ult i64 %__where_i_load483, %472
  br i1 %474, label %arr_ok487, label %arr_err488

arr_ok487:                                        ; preds = %arr_merge481
  %475 = getelementptr i8, ptr %473, i64 %__where_i_load483
  %476 = load i8, ptr %475, align 1
  br label %arr_merge489

arr_err488:                                       ; preds = %arr_merge481
  %477 = call i32 (ptr, ...) @printf(ptr @28)
  br label %arr_merge489

arr_merge489:                                     ; preds = %arr_ok487, %arr_err488
  %arr_val490 = phi i8 [ 0, %arr_err488 ], [ %476, %arr_ok487 ]
  %ptr_land_cover_class_8 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 27
  store i8 %arr_val490, ptr %ptr_land_cover_class_8, align 1
  %__where_i_load491 = load i64, ptr @__where_i, align 8
  %__where_src_load492 = load ptr, ptr @__where_src, align 8
  %df_data_ptr493 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load492, i32 0, i32 1
  %data_header494 = load ptr, ptr %df_data_ptr493, align 8
  %478 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header494, i32 0, i32 2
  %479 = load ptr, ptr %478, align 8
  %480 = getelementptr ptr, ptr %479, i64 28
  %481 = load ptr, ptr %480, align 8
  %482 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %481, i32 0, i32 0
  %483 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %481, i32 0, i32 2
  %484 = load i64, ptr %482, align 4
  %485 = load ptr, ptr %483, align 8
  %486 = icmp ult i64 %__where_i_load491, %484
  br i1 %486, label %arr_ok495, label %arr_err496

arr_ok495:                                        ; preds = %arr_merge489
  %487 = getelementptr i8, ptr %485, i64 %__where_i_load491
  %488 = load i8, ptr %487, align 1
  br label %arr_merge497

arr_err496:                                       ; preds = %arr_merge489
  %489 = call i32 (ptr, ...) @printf(ptr @29)
  br label %arr_merge497

arr_merge497:                                     ; preds = %arr_ok495, %arr_err496
  %arr_val498 = phi i8 [ 0, %arr_err496 ], [ %488, %arr_ok495 ]
  %ptr_land_cover_class_9 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 28
  store i8 %arr_val498, ptr %ptr_land_cover_class_9, align 1
  %__where_i_load499 = load i64, ptr @__where_i, align 8
  %__where_src_load500 = load ptr, ptr @__where_src, align 8
  %df_data_ptr501 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load500, i32 0, i32 1
  %data_header502 = load ptr, ptr %df_data_ptr501, align 8
  %490 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header502, i32 0, i32 2
  %491 = load ptr, ptr %490, align 8
  %492 = getelementptr ptr, ptr %491, i64 29
  %493 = load ptr, ptr %492, align 8
  %494 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %493, i32 0, i32 0
  %495 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %493, i32 0, i32 2
  %496 = load i64, ptr %494, align 4
  %497 = load ptr, ptr %495, align 8
  %498 = icmp ult i64 %__where_i_load499, %496
  br i1 %498, label %arr_ok503, label %arr_err504

arr_ok503:                                        ; preds = %arr_merge497
  %499 = getelementptr i8, ptr %497, i64 %__where_i_load499
  %500 = load i8, ptr %499, align 1
  br label %arr_merge505

arr_err504:                                       ; preds = %arr_merge497
  %501 = call i32 (ptr, ...) @printf(ptr @30)
  br label %arr_merge505

arr_merge505:                                     ; preds = %arr_ok503, %arr_err504
  %arr_val506 = phi i8 [ 0, %arr_err504 ], [ %500, %arr_ok503 ]
  %ptr_land_cover_class_10 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 29
  store i8 %arr_val506, ptr %ptr_land_cover_class_10, align 1
  %__where_i_load507 = load i64, ptr @__where_i, align 8
  %__where_src_load508 = load ptr, ptr @__where_src, align 8
  %df_data_ptr509 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load508, i32 0, i32 1
  %data_header510 = load ptr, ptr %df_data_ptr509, align 8
  %502 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header510, i32 0, i32 2
  %503 = load ptr, ptr %502, align 8
  %504 = getelementptr ptr, ptr %503, i64 30
  %505 = load ptr, ptr %504, align 8
  %506 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %505, i32 0, i32 0
  %507 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %505, i32 0, i32 2
  %508 = load i64, ptr %506, align 4
  %509 = load ptr, ptr %507, align 8
  %510 = icmp ult i64 %__where_i_load507, %508
  br i1 %510, label %arr_ok511, label %arr_err512

arr_ok511:                                        ; preds = %arr_merge505
  %511 = getelementptr i8, ptr %509, i64 %__where_i_load507
  %512 = load i8, ptr %511, align 1
  br label %arr_merge513

arr_err512:                                       ; preds = %arr_merge505
  %513 = call i32 (ptr, ...) @printf(ptr @31)
  br label %arr_merge513

arr_merge513:                                     ; preds = %arr_ok511, %arr_err512
  %arr_val514 = phi i8 [ 0, %arr_err512 ], [ %512, %arr_ok511 ]
  %ptr_land_cover_class_11 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 30
  store i8 %arr_val514, ptr %ptr_land_cover_class_11, align 1
  %__where_i_load515 = load i64, ptr @__where_i, align 8
  %__where_src_load516 = load ptr, ptr @__where_src, align 8
  %df_data_ptr517 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load516, i32 0, i32 1
  %data_header518 = load ptr, ptr %df_data_ptr517, align 8
  %514 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header518, i32 0, i32 2
  %515 = load ptr, ptr %514, align 8
  %516 = getelementptr ptr, ptr %515, i64 31
  %517 = load ptr, ptr %516, align 8
  %518 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %517, i32 0, i32 0
  %519 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %517, i32 0, i32 2
  %520 = load i64, ptr %518, align 4
  %521 = load ptr, ptr %519, align 8
  %522 = icmp ult i64 %__where_i_load515, %520
  br i1 %522, label %arr_ok519, label %arr_err520

arr_ok519:                                        ; preds = %arr_merge513
  %523 = getelementptr i8, ptr %521, i64 %__where_i_load515
  %524 = load i8, ptr %523, align 1
  br label %arr_merge521

arr_err520:                                       ; preds = %arr_merge513
  %525 = call i32 (ptr, ...) @printf(ptr @32)
  br label %arr_merge521

arr_merge521:                                     ; preds = %arr_ok519, %arr_err520
  %arr_val522 = phi i8 [ 0, %arr_err520 ], [ %524, %arr_ok519 ]
  %ptr_land_cover_class_12 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 31
  store i8 %arr_val522, ptr %ptr_land_cover_class_12, align 1
  %__where_i_load523 = load i64, ptr @__where_i, align 8
  %__where_src_load524 = load ptr, ptr @__where_src, align 8
  %df_data_ptr525 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load524, i32 0, i32 1
  %data_header526 = load ptr, ptr %df_data_ptr525, align 8
  %526 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header526, i32 0, i32 2
  %527 = load ptr, ptr %526, align 8
  %528 = getelementptr ptr, ptr %527, i64 32
  %529 = load ptr, ptr %528, align 8
  %530 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %529, i32 0, i32 0
  %531 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %529, i32 0, i32 2
  %532 = load i64, ptr %530, align 4
  %533 = load ptr, ptr %531, align 8
  %534 = icmp ult i64 %__where_i_load523, %532
  br i1 %534, label %arr_ok527, label %arr_err528

arr_ok527:                                        ; preds = %arr_merge521
  %535 = getelementptr i8, ptr %533, i64 %__where_i_load523
  %536 = load i8, ptr %535, align 1
  br label %arr_merge529

arr_err528:                                       ; preds = %arr_merge521
  %537 = call i32 (ptr, ...) @printf(ptr @33)
  br label %arr_merge529

arr_merge529:                                     ; preds = %arr_ok527, %arr_err528
  %arr_val530 = phi i8 [ 0, %arr_err528 ], [ %536, %arr_ok527 ]
  %ptr_land_cover_class_13 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 32
  store i8 %arr_val530, ptr %ptr_land_cover_class_13, align 1
  %__where_i_load531 = load i64, ptr @__where_i, align 8
  %__where_src_load532 = load ptr, ptr @__where_src, align 8
  %df_data_ptr533 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load532, i32 0, i32 1
  %data_header534 = load ptr, ptr %df_data_ptr533, align 8
  %538 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header534, i32 0, i32 2
  %539 = load ptr, ptr %538, align 8
  %540 = getelementptr ptr, ptr %539, i64 33
  %541 = load ptr, ptr %540, align 8
  %542 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %541, i32 0, i32 0
  %543 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %541, i32 0, i32 2
  %544 = load i64, ptr %542, align 4
  %545 = load ptr, ptr %543, align 8
  %546 = icmp ult i64 %__where_i_load531, %544
  br i1 %546, label %arr_ok535, label %arr_err536

arr_ok535:                                        ; preds = %arr_merge529
  %547 = getelementptr i8, ptr %545, i64 %__where_i_load531
  %548 = load i8, ptr %547, align 1
  br label %arr_merge537

arr_err536:                                       ; preds = %arr_merge529
  %549 = call i32 (ptr, ...) @printf(ptr @34)
  br label %arr_merge537

arr_merge537:                                     ; preds = %arr_ok535, %arr_err536
  %arr_val538 = phi i8 [ 0, %arr_err536 ], [ %548, %arr_ok535 ]
  %ptr_land_cover_class_14 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 33
  store i8 %arr_val538, ptr %ptr_land_cover_class_14, align 1
  %__where_i_load539 = load i64, ptr @__where_i, align 8
  %__where_src_load540 = load ptr, ptr @__where_src, align 8
  %df_data_ptr541 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load540, i32 0, i32 1
  %data_header542 = load ptr, ptr %df_data_ptr541, align 8
  %550 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header542, i32 0, i32 2
  %551 = load ptr, ptr %550, align 8
  %552 = getelementptr ptr, ptr %551, i64 34
  %553 = load ptr, ptr %552, align 8
  %554 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %553, i32 0, i32 0
  %555 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %553, i32 0, i32 2
  %556 = load i64, ptr %554, align 4
  %557 = load ptr, ptr %555, align 8
  %558 = icmp ult i64 %__where_i_load539, %556
  br i1 %558, label %arr_ok543, label %arr_err544

arr_ok543:                                        ; preds = %arr_merge537
  %559 = getelementptr i8, ptr %557, i64 %__where_i_load539
  %560 = load i8, ptr %559, align 1
  br label %arr_merge545

arr_err544:                                       ; preds = %arr_merge537
  %561 = call i32 (ptr, ...) @printf(ptr @35)
  br label %arr_merge545

arr_merge545:                                     ; preds = %arr_ok543, %arr_err544
  %arr_val546 = phi i8 [ 0, %arr_err544 ], [ %560, %arr_ok543 ]
  %ptr_land_cover_class_15 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 34
  store i8 %arr_val546, ptr %ptr_land_cover_class_15, align 1
  %__where_i_load547 = load i64, ptr @__where_i, align 8
  %__where_src_load548 = load ptr, ptr @__where_src, align 8
  %df_data_ptr549 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load548, i32 0, i32 1
  %data_header550 = load ptr, ptr %df_data_ptr549, align 8
  %562 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header550, i32 0, i32 2
  %563 = load ptr, ptr %562, align 8
  %564 = getelementptr ptr, ptr %563, i64 35
  %565 = load ptr, ptr %564, align 8
  %566 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %565, i32 0, i32 0
  %567 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %565, i32 0, i32 2
  %568 = load i64, ptr %566, align 4
  %569 = load ptr, ptr %567, align 8
  %570 = icmp ult i64 %__where_i_load547, %568
  br i1 %570, label %arr_ok551, label %arr_err552

arr_ok551:                                        ; preds = %arr_merge545
  %571 = getelementptr i8, ptr %569, i64 %__where_i_load547
  %572 = load i8, ptr %571, align 1
  br label %arr_merge553

arr_err552:                                       ; preds = %arr_merge545
  %573 = call i32 (ptr, ...) @printf(ptr @36)
  br label %arr_merge553

arr_merge553:                                     ; preds = %arr_ok551, %arr_err552
  %arr_val554 = phi i8 [ 0, %arr_err552 ], [ %572, %arr_ok551 ]
  %ptr_land_cover_class_16 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 35
  store i8 %arr_val554, ptr %ptr_land_cover_class_16, align 1
  %__where_i_load555 = load i64, ptr @__where_i, align 8
  %__where_src_load556 = load ptr, ptr @__where_src, align 8
  %df_data_ptr557 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load556, i32 0, i32 1
  %data_header558 = load ptr, ptr %df_data_ptr557, align 8
  %574 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header558, i32 0, i32 2
  %575 = load ptr, ptr %574, align 8
  %576 = getelementptr ptr, ptr %575, i64 36
  %577 = load ptr, ptr %576, align 8
  %578 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %577, i32 0, i32 0
  %579 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %577, i32 0, i32 2
  %580 = load i64, ptr %578, align 4
  %581 = load ptr, ptr %579, align 8
  %582 = icmp ult i64 %__where_i_load555, %580
  br i1 %582, label %arr_ok559, label %arr_err560

arr_ok559:                                        ; preds = %arr_merge553
  %583 = getelementptr i8, ptr %581, i64 %__where_i_load555
  %584 = load i8, ptr %583, align 1
  br label %arr_merge561

arr_err560:                                       ; preds = %arr_merge553
  %585 = call i32 (ptr, ...) @printf(ptr @37)
  br label %arr_merge561

arr_merge561:                                     ; preds = %arr_ok559, %arr_err560
  %arr_val562 = phi i8 [ 0, %arr_err560 ], [ %584, %arr_ok559 ]
  %ptr_land_cover_class_17 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 36
  store i8 %arr_val562, ptr %ptr_land_cover_class_17, align 1
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %df_cast = bitcast ptr %__where_result_load to ptr
  %586 = getelementptr inbounds nuw %dataframe, ptr %df_cast, i32 0, i32 1
  %587 = load ptr, ptr %586, align 8
  %data_array = bitcast ptr %587 to ptr
  %588 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_array, i32 0, i32 2
  %589 = load ptr, ptr %588, align 8
  %590 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 0
  %591 = load ptr, ptr %590, align 8
  %col_ptr_ptr = getelementptr ptr, ptr %589, i64 0
  %592 = load ptr, ptr %col_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %592, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %592, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %592, i32 0, i32 2
  %curr_len = load i64, ptr %len_ptr, align 4
  %curr_cap = load i64, ptr %cap_ptr, align 4
  %curr_data = load ptr, ptr %data_ptr_ptr, align 8
  %needs_grow = icmp sge i64 %curr_len, %curr_cap
  br i1 %needs_grow, label %grow, label %store_element

grow:                                             ; preds = %arr_merge561
  %593 = icmp eq i64 %curr_cap, 0
  %594 = mul i64 %curr_cap, 2
  %new_cap = select i1 %593, i64 4, i64 %594
  %new_byte_count = mul i64 %new_cap, 8
  %reallocated_data = call ptr @realloc(ptr %curr_data, i64 %new_byte_count)
  store i64 %new_cap, ptr %cap_ptr, align 8
  store ptr %reallocated_data, ptr %data_ptr_ptr, align 8
  br label %store_element

store_element:                                    ; preds = %grow, %arr_merge561
  %final_data = load ptr, ptr %data_ptr_ptr, align 8
  %offset = mul i64 %curr_len, 8
  %raw_elem_ptr = getelementptr i8, ptr %final_data, i64 %offset
  store ptr %591, ptr %raw_elem_ptr, align 8
  %new_len = add i64 %curr_len, 1
  store i64 %new_len, ptr %len_ptr, align 8
  %595 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 1
  %596 = load double, ptr %595, align 8
  %col_ptr_ptr563 = getelementptr ptr, ptr %589, i64 1
  %597 = load ptr, ptr %col_ptr_ptr563, align 8
  %len_ptr564 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %597, i32 0, i32 0
  %cap_ptr565 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %597, i32 0, i32 1
  %data_ptr_ptr566 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %597, i32 0, i32 2
  %curr_len567 = load i64, ptr %len_ptr564, align 4
  %curr_cap568 = load i64, ptr %cap_ptr565, align 4
  %curr_data569 = load ptr, ptr %data_ptr_ptr566, align 8
  %needs_grow570 = icmp sge i64 %curr_len567, %curr_cap568
  br i1 %needs_grow570, label %grow571, label %store_element572

grow571:                                          ; preds = %store_element
  %598 = icmp eq i64 %curr_cap568, 0
  %599 = mul i64 %curr_cap568, 2
  %new_cap573 = select i1 %598, i64 4, i64 %599
  %new_byte_count574 = mul i64 %new_cap573, 8
  %reallocated_data575 = call ptr @realloc(ptr %curr_data569, i64 %new_byte_count574)
  store i64 %new_cap573, ptr %cap_ptr565, align 8
  store ptr %reallocated_data575, ptr %data_ptr_ptr566, align 8
  br label %store_element572

store_element572:                                 ; preds = %grow571, %store_element
  %final_data576 = load ptr, ptr %data_ptr_ptr566, align 8
  %offset577 = mul i64 %curr_len567, 8
  %raw_elem_ptr578 = getelementptr i8, ptr %final_data576, i64 %offset577
  store double %596, ptr %raw_elem_ptr578, align 8
  %new_len579 = add i64 %curr_len567, 1
  store i64 %new_len579, ptr %len_ptr564, align 8
  %600 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 2
  %601 = load double, ptr %600, align 8
  %col_ptr_ptr580 = getelementptr ptr, ptr %589, i64 2
  %602 = load ptr, ptr %col_ptr_ptr580, align 8
  %len_ptr581 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %602, i32 0, i32 0
  %cap_ptr582 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %602, i32 0, i32 1
  %data_ptr_ptr583 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %602, i32 0, i32 2
  %curr_len584 = load i64, ptr %len_ptr581, align 4
  %curr_cap585 = load i64, ptr %cap_ptr582, align 4
  %curr_data586 = load ptr, ptr %data_ptr_ptr583, align 8
  %needs_grow587 = icmp sge i64 %curr_len584, %curr_cap585
  br i1 %needs_grow587, label %grow588, label %store_element589

grow588:                                          ; preds = %store_element572
  %603 = icmp eq i64 %curr_cap585, 0
  %604 = mul i64 %curr_cap585, 2
  %new_cap590 = select i1 %603, i64 4, i64 %604
  %new_byte_count591 = mul i64 %new_cap590, 8
  %reallocated_data592 = call ptr @realloc(ptr %curr_data586, i64 %new_byte_count591)
  store i64 %new_cap590, ptr %cap_ptr582, align 8
  store ptr %reallocated_data592, ptr %data_ptr_ptr583, align 8
  br label %store_element589

store_element589:                                 ; preds = %grow588, %store_element572
  %final_data593 = load ptr, ptr %data_ptr_ptr583, align 8
  %offset594 = mul i64 %curr_len584, 8
  %raw_elem_ptr595 = getelementptr i8, ptr %final_data593, i64 %offset594
  store double %601, ptr %raw_elem_ptr595, align 8
  %new_len596 = add i64 %curr_len584, 1
  store i64 %new_len596, ptr %len_ptr581, align 8
  %605 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 3
  %606 = load double, ptr %605, align 8
  %col_ptr_ptr597 = getelementptr ptr, ptr %589, i64 3
  %607 = load ptr, ptr %col_ptr_ptr597, align 8
  %len_ptr598 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %607, i32 0, i32 0
  %cap_ptr599 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %607, i32 0, i32 1
  %data_ptr_ptr600 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %607, i32 0, i32 2
  %curr_len601 = load i64, ptr %len_ptr598, align 4
  %curr_cap602 = load i64, ptr %cap_ptr599, align 4
  %curr_data603 = load ptr, ptr %data_ptr_ptr600, align 8
  %needs_grow604 = icmp sge i64 %curr_len601, %curr_cap602
  br i1 %needs_grow604, label %grow605, label %store_element606

grow605:                                          ; preds = %store_element589
  %608 = icmp eq i64 %curr_cap602, 0
  %609 = mul i64 %curr_cap602, 2
  %new_cap607 = select i1 %608, i64 4, i64 %609
  %new_byte_count608 = mul i64 %new_cap607, 8
  %reallocated_data609 = call ptr @realloc(ptr %curr_data603, i64 %new_byte_count608)
  store i64 %new_cap607, ptr %cap_ptr599, align 8
  store ptr %reallocated_data609, ptr %data_ptr_ptr600, align 8
  br label %store_element606

store_element606:                                 ; preds = %grow605, %store_element589
  %final_data610 = load ptr, ptr %data_ptr_ptr600, align 8
  %offset611 = mul i64 %curr_len601, 8
  %raw_elem_ptr612 = getelementptr i8, ptr %final_data610, i64 %offset611
  store double %606, ptr %raw_elem_ptr612, align 8
  %new_len613 = add i64 %curr_len601, 1
  store i64 %new_len613, ptr %len_ptr598, align 8
  %610 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 4
  %611 = load double, ptr %610, align 8
  %col_ptr_ptr614 = getelementptr ptr, ptr %589, i64 4
  %612 = load ptr, ptr %col_ptr_ptr614, align 8
  %len_ptr615 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %612, i32 0, i32 0
  %cap_ptr616 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %612, i32 0, i32 1
  %data_ptr_ptr617 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %612, i32 0, i32 2
  %curr_len618 = load i64, ptr %len_ptr615, align 4
  %curr_cap619 = load i64, ptr %cap_ptr616, align 4
  %curr_data620 = load ptr, ptr %data_ptr_ptr617, align 8
  %needs_grow621 = icmp sge i64 %curr_len618, %curr_cap619
  br i1 %needs_grow621, label %grow622, label %store_element623

grow622:                                          ; preds = %store_element606
  %613 = icmp eq i64 %curr_cap619, 0
  %614 = mul i64 %curr_cap619, 2
  %new_cap624 = select i1 %613, i64 4, i64 %614
  %new_byte_count625 = mul i64 %new_cap624, 8
  %reallocated_data626 = call ptr @realloc(ptr %curr_data620, i64 %new_byte_count625)
  store i64 %new_cap624, ptr %cap_ptr616, align 8
  store ptr %reallocated_data626, ptr %data_ptr_ptr617, align 8
  br label %store_element623

store_element623:                                 ; preds = %grow622, %store_element606
  %final_data627 = load ptr, ptr %data_ptr_ptr617, align 8
  %offset628 = mul i64 %curr_len618, 8
  %raw_elem_ptr629 = getelementptr i8, ptr %final_data627, i64 %offset628
  store double %611, ptr %raw_elem_ptr629, align 8
  %new_len630 = add i64 %curr_len618, 1
  store i64 %new_len630, ptr %len_ptr615, align 8
  %615 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 5
  %616 = load double, ptr %615, align 8
  %col_ptr_ptr631 = getelementptr ptr, ptr %589, i64 5
  %617 = load ptr, ptr %col_ptr_ptr631, align 8
  %len_ptr632 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %617, i32 0, i32 0
  %cap_ptr633 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %617, i32 0, i32 1
  %data_ptr_ptr634 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %617, i32 0, i32 2
  %curr_len635 = load i64, ptr %len_ptr632, align 4
  %curr_cap636 = load i64, ptr %cap_ptr633, align 4
  %curr_data637 = load ptr, ptr %data_ptr_ptr634, align 8
  %needs_grow638 = icmp sge i64 %curr_len635, %curr_cap636
  br i1 %needs_grow638, label %grow639, label %store_element640

grow639:                                          ; preds = %store_element623
  %618 = icmp eq i64 %curr_cap636, 0
  %619 = mul i64 %curr_cap636, 2
  %new_cap641 = select i1 %618, i64 4, i64 %619
  %new_byte_count642 = mul i64 %new_cap641, 8
  %reallocated_data643 = call ptr @realloc(ptr %curr_data637, i64 %new_byte_count642)
  store i64 %new_cap641, ptr %cap_ptr633, align 8
  store ptr %reallocated_data643, ptr %data_ptr_ptr634, align 8
  br label %store_element640

store_element640:                                 ; preds = %grow639, %store_element623
  %final_data644 = load ptr, ptr %data_ptr_ptr634, align 8
  %offset645 = mul i64 %curr_len635, 8
  %raw_elem_ptr646 = getelementptr i8, ptr %final_data644, i64 %offset645
  store double %616, ptr %raw_elem_ptr646, align 8
  %new_len647 = add i64 %curr_len635, 1
  store i64 %new_len647, ptr %len_ptr632, align 8
  %620 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 6
  %621 = load double, ptr %620, align 8
  %col_ptr_ptr648 = getelementptr ptr, ptr %589, i64 6
  %622 = load ptr, ptr %col_ptr_ptr648, align 8
  %len_ptr649 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %622, i32 0, i32 0
  %cap_ptr650 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %622, i32 0, i32 1
  %data_ptr_ptr651 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %622, i32 0, i32 2
  %curr_len652 = load i64, ptr %len_ptr649, align 4
  %curr_cap653 = load i64, ptr %cap_ptr650, align 4
  %curr_data654 = load ptr, ptr %data_ptr_ptr651, align 8
  %needs_grow655 = icmp sge i64 %curr_len652, %curr_cap653
  br i1 %needs_grow655, label %grow656, label %store_element657

grow656:                                          ; preds = %store_element640
  %623 = icmp eq i64 %curr_cap653, 0
  %624 = mul i64 %curr_cap653, 2
  %new_cap658 = select i1 %623, i64 4, i64 %624
  %new_byte_count659 = mul i64 %new_cap658, 8
  %reallocated_data660 = call ptr @realloc(ptr %curr_data654, i64 %new_byte_count659)
  store i64 %new_cap658, ptr %cap_ptr650, align 8
  store ptr %reallocated_data660, ptr %data_ptr_ptr651, align 8
  br label %store_element657

store_element657:                                 ; preds = %grow656, %store_element640
  %final_data661 = load ptr, ptr %data_ptr_ptr651, align 8
  %offset662 = mul i64 %curr_len652, 8
  %raw_elem_ptr663 = getelementptr i8, ptr %final_data661, i64 %offset662
  store double %621, ptr %raw_elem_ptr663, align 8
  %new_len664 = add i64 %curr_len652, 1
  store i64 %new_len664, ptr %len_ptr649, align 8
  %625 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 7
  %626 = load double, ptr %625, align 8
  %col_ptr_ptr665 = getelementptr ptr, ptr %589, i64 7
  %627 = load ptr, ptr %col_ptr_ptr665, align 8
  %len_ptr666 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %627, i32 0, i32 0
  %cap_ptr667 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %627, i32 0, i32 1
  %data_ptr_ptr668 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %627, i32 0, i32 2
  %curr_len669 = load i64, ptr %len_ptr666, align 4
  %curr_cap670 = load i64, ptr %cap_ptr667, align 4
  %curr_data671 = load ptr, ptr %data_ptr_ptr668, align 8
  %needs_grow672 = icmp sge i64 %curr_len669, %curr_cap670
  br i1 %needs_grow672, label %grow673, label %store_element674

grow673:                                          ; preds = %store_element657
  %628 = icmp eq i64 %curr_cap670, 0
  %629 = mul i64 %curr_cap670, 2
  %new_cap675 = select i1 %628, i64 4, i64 %629
  %new_byte_count676 = mul i64 %new_cap675, 8
  %reallocated_data677 = call ptr @realloc(ptr %curr_data671, i64 %new_byte_count676)
  store i64 %new_cap675, ptr %cap_ptr667, align 8
  store ptr %reallocated_data677, ptr %data_ptr_ptr668, align 8
  br label %store_element674

store_element674:                                 ; preds = %grow673, %store_element657
  %final_data678 = load ptr, ptr %data_ptr_ptr668, align 8
  %offset679 = mul i64 %curr_len669, 8
  %raw_elem_ptr680 = getelementptr i8, ptr %final_data678, i64 %offset679
  store double %626, ptr %raw_elem_ptr680, align 8
  %new_len681 = add i64 %curr_len669, 1
  store i64 %new_len681, ptr %len_ptr666, align 8
  %630 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 8
  %631 = load double, ptr %630, align 8
  %col_ptr_ptr682 = getelementptr ptr, ptr %589, i64 8
  %632 = load ptr, ptr %col_ptr_ptr682, align 8
  %len_ptr683 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %632, i32 0, i32 0
  %cap_ptr684 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %632, i32 0, i32 1
  %data_ptr_ptr685 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %632, i32 0, i32 2
  %curr_len686 = load i64, ptr %len_ptr683, align 4
  %curr_cap687 = load i64, ptr %cap_ptr684, align 4
  %curr_data688 = load ptr, ptr %data_ptr_ptr685, align 8
  %needs_grow689 = icmp sge i64 %curr_len686, %curr_cap687
  br i1 %needs_grow689, label %grow690, label %store_element691

grow690:                                          ; preds = %store_element674
  %633 = icmp eq i64 %curr_cap687, 0
  %634 = mul i64 %curr_cap687, 2
  %new_cap692 = select i1 %633, i64 4, i64 %634
  %new_byte_count693 = mul i64 %new_cap692, 8
  %reallocated_data694 = call ptr @realloc(ptr %curr_data688, i64 %new_byte_count693)
  store i64 %new_cap692, ptr %cap_ptr684, align 8
  store ptr %reallocated_data694, ptr %data_ptr_ptr685, align 8
  br label %store_element691

store_element691:                                 ; preds = %grow690, %store_element674
  %final_data695 = load ptr, ptr %data_ptr_ptr685, align 8
  %offset696 = mul i64 %curr_len686, 8
  %raw_elem_ptr697 = getelementptr i8, ptr %final_data695, i64 %offset696
  store double %631, ptr %raw_elem_ptr697, align 8
  %new_len698 = add i64 %curr_len686, 1
  store i64 %new_len698, ptr %len_ptr683, align 8
  %635 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 9
  %636 = load double, ptr %635, align 8
  %col_ptr_ptr699 = getelementptr ptr, ptr %589, i64 9
  %637 = load ptr, ptr %col_ptr_ptr699, align 8
  %len_ptr700 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %637, i32 0, i32 0
  %cap_ptr701 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %637, i32 0, i32 1
  %data_ptr_ptr702 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %637, i32 0, i32 2
  %curr_len703 = load i64, ptr %len_ptr700, align 4
  %curr_cap704 = load i64, ptr %cap_ptr701, align 4
  %curr_data705 = load ptr, ptr %data_ptr_ptr702, align 8
  %needs_grow706 = icmp sge i64 %curr_len703, %curr_cap704
  br i1 %needs_grow706, label %grow707, label %store_element708

grow707:                                          ; preds = %store_element691
  %638 = icmp eq i64 %curr_cap704, 0
  %639 = mul i64 %curr_cap704, 2
  %new_cap709 = select i1 %638, i64 4, i64 %639
  %new_byte_count710 = mul i64 %new_cap709, 8
  %reallocated_data711 = call ptr @realloc(ptr %curr_data705, i64 %new_byte_count710)
  store i64 %new_cap709, ptr %cap_ptr701, align 8
  store ptr %reallocated_data711, ptr %data_ptr_ptr702, align 8
  br label %store_element708

store_element708:                                 ; preds = %grow707, %store_element691
  %final_data712 = load ptr, ptr %data_ptr_ptr702, align 8
  %offset713 = mul i64 %curr_len703, 8
  %raw_elem_ptr714 = getelementptr i8, ptr %final_data712, i64 %offset713
  store double %636, ptr %raw_elem_ptr714, align 8
  %new_len715 = add i64 %curr_len703, 1
  store i64 %new_len715, ptr %len_ptr700, align 8
  %640 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 10
  %641 = load double, ptr %640, align 8
  %col_ptr_ptr716 = getelementptr ptr, ptr %589, i64 10
  %642 = load ptr, ptr %col_ptr_ptr716, align 8
  %len_ptr717 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %642, i32 0, i32 0
  %cap_ptr718 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %642, i32 0, i32 1
  %data_ptr_ptr719 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %642, i32 0, i32 2
  %curr_len720 = load i64, ptr %len_ptr717, align 4
  %curr_cap721 = load i64, ptr %cap_ptr718, align 4
  %curr_data722 = load ptr, ptr %data_ptr_ptr719, align 8
  %needs_grow723 = icmp sge i64 %curr_len720, %curr_cap721
  br i1 %needs_grow723, label %grow724, label %store_element725

grow724:                                          ; preds = %store_element708
  %643 = icmp eq i64 %curr_cap721, 0
  %644 = mul i64 %curr_cap721, 2
  %new_cap726 = select i1 %643, i64 4, i64 %644
  %new_byte_count727 = mul i64 %new_cap726, 8
  %reallocated_data728 = call ptr @realloc(ptr %curr_data722, i64 %new_byte_count727)
  store i64 %new_cap726, ptr %cap_ptr718, align 8
  store ptr %reallocated_data728, ptr %data_ptr_ptr719, align 8
  br label %store_element725

store_element725:                                 ; preds = %grow724, %store_element708
  %final_data729 = load ptr, ptr %data_ptr_ptr719, align 8
  %offset730 = mul i64 %curr_len720, 8
  %raw_elem_ptr731 = getelementptr i8, ptr %final_data729, i64 %offset730
  store double %641, ptr %raw_elem_ptr731, align 8
  %new_len732 = add i64 %curr_len720, 1
  store i64 %new_len732, ptr %len_ptr717, align 8
  %645 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 11
  %646 = load double, ptr %645, align 8
  %col_ptr_ptr733 = getelementptr ptr, ptr %589, i64 11
  %647 = load ptr, ptr %col_ptr_ptr733, align 8
  %len_ptr734 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %647, i32 0, i32 0
  %cap_ptr735 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %647, i32 0, i32 1
  %data_ptr_ptr736 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %647, i32 0, i32 2
  %curr_len737 = load i64, ptr %len_ptr734, align 4
  %curr_cap738 = load i64, ptr %cap_ptr735, align 4
  %curr_data739 = load ptr, ptr %data_ptr_ptr736, align 8
  %needs_grow740 = icmp sge i64 %curr_len737, %curr_cap738
  br i1 %needs_grow740, label %grow741, label %store_element742

grow741:                                          ; preds = %store_element725
  %648 = icmp eq i64 %curr_cap738, 0
  %649 = mul i64 %curr_cap738, 2
  %new_cap743 = select i1 %648, i64 4, i64 %649
  %new_byte_count744 = mul i64 %new_cap743, 8
  %reallocated_data745 = call ptr @realloc(ptr %curr_data739, i64 %new_byte_count744)
  store i64 %new_cap743, ptr %cap_ptr735, align 8
  store ptr %reallocated_data745, ptr %data_ptr_ptr736, align 8
  br label %store_element742

store_element742:                                 ; preds = %grow741, %store_element725
  %final_data746 = load ptr, ptr %data_ptr_ptr736, align 8
  %offset747 = mul i64 %curr_len737, 8
  %raw_elem_ptr748 = getelementptr i8, ptr %final_data746, i64 %offset747
  store double %646, ptr %raw_elem_ptr748, align 8
  %new_len749 = add i64 %curr_len737, 1
  store i64 %new_len749, ptr %len_ptr734, align 8
  %650 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 12
  %651 = load double, ptr %650, align 8
  %col_ptr_ptr750 = getelementptr ptr, ptr %589, i64 12
  %652 = load ptr, ptr %col_ptr_ptr750, align 8
  %len_ptr751 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %652, i32 0, i32 0
  %cap_ptr752 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %652, i32 0, i32 1
  %data_ptr_ptr753 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %652, i32 0, i32 2
  %curr_len754 = load i64, ptr %len_ptr751, align 4
  %curr_cap755 = load i64, ptr %cap_ptr752, align 4
  %curr_data756 = load ptr, ptr %data_ptr_ptr753, align 8
  %needs_grow757 = icmp sge i64 %curr_len754, %curr_cap755
  br i1 %needs_grow757, label %grow758, label %store_element759

grow758:                                          ; preds = %store_element742
  %653 = icmp eq i64 %curr_cap755, 0
  %654 = mul i64 %curr_cap755, 2
  %new_cap760 = select i1 %653, i64 4, i64 %654
  %new_byte_count761 = mul i64 %new_cap760, 8
  %reallocated_data762 = call ptr @realloc(ptr %curr_data756, i64 %new_byte_count761)
  store i64 %new_cap760, ptr %cap_ptr752, align 8
  store ptr %reallocated_data762, ptr %data_ptr_ptr753, align 8
  br label %store_element759

store_element759:                                 ; preds = %grow758, %store_element742
  %final_data763 = load ptr, ptr %data_ptr_ptr753, align 8
  %offset764 = mul i64 %curr_len754, 8
  %raw_elem_ptr765 = getelementptr i8, ptr %final_data763, i64 %offset764
  store double %651, ptr %raw_elem_ptr765, align 8
  %new_len766 = add i64 %curr_len754, 1
  store i64 %new_len766, ptr %len_ptr751, align 8
  %655 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 13
  %656 = load double, ptr %655, align 8
  %col_ptr_ptr767 = getelementptr ptr, ptr %589, i64 13
  %657 = load ptr, ptr %col_ptr_ptr767, align 8
  %len_ptr768 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %657, i32 0, i32 0
  %cap_ptr769 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %657, i32 0, i32 1
  %data_ptr_ptr770 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %657, i32 0, i32 2
  %curr_len771 = load i64, ptr %len_ptr768, align 4
  %curr_cap772 = load i64, ptr %cap_ptr769, align 4
  %curr_data773 = load ptr, ptr %data_ptr_ptr770, align 8
  %needs_grow774 = icmp sge i64 %curr_len771, %curr_cap772
  br i1 %needs_grow774, label %grow775, label %store_element776

grow775:                                          ; preds = %store_element759
  %658 = icmp eq i64 %curr_cap772, 0
  %659 = mul i64 %curr_cap772, 2
  %new_cap777 = select i1 %658, i64 4, i64 %659
  %new_byte_count778 = mul i64 %new_cap777, 8
  %reallocated_data779 = call ptr @realloc(ptr %curr_data773, i64 %new_byte_count778)
  store i64 %new_cap777, ptr %cap_ptr769, align 8
  store ptr %reallocated_data779, ptr %data_ptr_ptr770, align 8
  br label %store_element776

store_element776:                                 ; preds = %grow775, %store_element759
  %final_data780 = load ptr, ptr %data_ptr_ptr770, align 8
  %offset781 = mul i64 %curr_len771, 8
  %raw_elem_ptr782 = getelementptr i8, ptr %final_data780, i64 %offset781
  store double %656, ptr %raw_elem_ptr782, align 8
  %new_len783 = add i64 %curr_len771, 1
  store i64 %new_len783, ptr %len_ptr768, align 8
  %660 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 14
  %661 = load double, ptr %660, align 8
  %col_ptr_ptr784 = getelementptr ptr, ptr %589, i64 14
  %662 = load ptr, ptr %col_ptr_ptr784, align 8
  %len_ptr785 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %662, i32 0, i32 0
  %cap_ptr786 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %662, i32 0, i32 1
  %data_ptr_ptr787 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %662, i32 0, i32 2
  %curr_len788 = load i64, ptr %len_ptr785, align 4
  %curr_cap789 = load i64, ptr %cap_ptr786, align 4
  %curr_data790 = load ptr, ptr %data_ptr_ptr787, align 8
  %needs_grow791 = icmp sge i64 %curr_len788, %curr_cap789
  br i1 %needs_grow791, label %grow792, label %store_element793

grow792:                                          ; preds = %store_element776
  %663 = icmp eq i64 %curr_cap789, 0
  %664 = mul i64 %curr_cap789, 2
  %new_cap794 = select i1 %663, i64 4, i64 %664
  %new_byte_count795 = mul i64 %new_cap794, 8
  %reallocated_data796 = call ptr @realloc(ptr %curr_data790, i64 %new_byte_count795)
  store i64 %new_cap794, ptr %cap_ptr786, align 8
  store ptr %reallocated_data796, ptr %data_ptr_ptr787, align 8
  br label %store_element793

store_element793:                                 ; preds = %grow792, %store_element776
  %final_data797 = load ptr, ptr %data_ptr_ptr787, align 8
  %offset798 = mul i64 %curr_len788, 8
  %raw_elem_ptr799 = getelementptr i8, ptr %final_data797, i64 %offset798
  store double %661, ptr %raw_elem_ptr799, align 8
  %new_len800 = add i64 %curr_len788, 1
  store i64 %new_len800, ptr %len_ptr785, align 8
  %665 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 15
  %666 = load double, ptr %665, align 8
  %col_ptr_ptr801 = getelementptr ptr, ptr %589, i64 15
  %667 = load ptr, ptr %col_ptr_ptr801, align 8
  %len_ptr802 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %667, i32 0, i32 0
  %cap_ptr803 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %667, i32 0, i32 1
  %data_ptr_ptr804 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %667, i32 0, i32 2
  %curr_len805 = load i64, ptr %len_ptr802, align 4
  %curr_cap806 = load i64, ptr %cap_ptr803, align 4
  %curr_data807 = load ptr, ptr %data_ptr_ptr804, align 8
  %needs_grow808 = icmp sge i64 %curr_len805, %curr_cap806
  br i1 %needs_grow808, label %grow809, label %store_element810

grow809:                                          ; preds = %store_element793
  %668 = icmp eq i64 %curr_cap806, 0
  %669 = mul i64 %curr_cap806, 2
  %new_cap811 = select i1 %668, i64 4, i64 %669
  %new_byte_count812 = mul i64 %new_cap811, 8
  %reallocated_data813 = call ptr @realloc(ptr %curr_data807, i64 %new_byte_count812)
  store i64 %new_cap811, ptr %cap_ptr803, align 8
  store ptr %reallocated_data813, ptr %data_ptr_ptr804, align 8
  br label %store_element810

store_element810:                                 ; preds = %grow809, %store_element793
  %final_data814 = load ptr, ptr %data_ptr_ptr804, align 8
  %offset815 = mul i64 %curr_len805, 8
  %raw_elem_ptr816 = getelementptr i8, ptr %final_data814, i64 %offset815
  store double %666, ptr %raw_elem_ptr816, align 8
  %new_len817 = add i64 %curr_len805, 1
  store i64 %new_len817, ptr %len_ptr802, align 8
  %670 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 16
  %671 = load double, ptr %670, align 8
  %col_ptr_ptr818 = getelementptr ptr, ptr %589, i64 16
  %672 = load ptr, ptr %col_ptr_ptr818, align 8
  %len_ptr819 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %672, i32 0, i32 0
  %cap_ptr820 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %672, i32 0, i32 1
  %data_ptr_ptr821 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %672, i32 0, i32 2
  %curr_len822 = load i64, ptr %len_ptr819, align 4
  %curr_cap823 = load i64, ptr %cap_ptr820, align 4
  %curr_data824 = load ptr, ptr %data_ptr_ptr821, align 8
  %needs_grow825 = icmp sge i64 %curr_len822, %curr_cap823
  br i1 %needs_grow825, label %grow826, label %store_element827

grow826:                                          ; preds = %store_element810
  %673 = icmp eq i64 %curr_cap823, 0
  %674 = mul i64 %curr_cap823, 2
  %new_cap828 = select i1 %673, i64 4, i64 %674
  %new_byte_count829 = mul i64 %new_cap828, 8
  %reallocated_data830 = call ptr @realloc(ptr %curr_data824, i64 %new_byte_count829)
  store i64 %new_cap828, ptr %cap_ptr820, align 8
  store ptr %reallocated_data830, ptr %data_ptr_ptr821, align 8
  br label %store_element827

store_element827:                                 ; preds = %grow826, %store_element810
  %final_data831 = load ptr, ptr %data_ptr_ptr821, align 8
  %offset832 = mul i64 %curr_len822, 8
  %raw_elem_ptr833 = getelementptr i8, ptr %final_data831, i64 %offset832
  store double %671, ptr %raw_elem_ptr833, align 8
  %new_len834 = add i64 %curr_len822, 1
  store i64 %new_len834, ptr %len_ptr819, align 8
  %675 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 17
  %676 = load double, ptr %675, align 8
  %col_ptr_ptr835 = getelementptr ptr, ptr %589, i64 17
  %677 = load ptr, ptr %col_ptr_ptr835, align 8
  %len_ptr836 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %677, i32 0, i32 0
  %cap_ptr837 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %677, i32 0, i32 1
  %data_ptr_ptr838 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %677, i32 0, i32 2
  %curr_len839 = load i64, ptr %len_ptr836, align 4
  %curr_cap840 = load i64, ptr %cap_ptr837, align 4
  %curr_data841 = load ptr, ptr %data_ptr_ptr838, align 8
  %needs_grow842 = icmp sge i64 %curr_len839, %curr_cap840
  br i1 %needs_grow842, label %grow843, label %store_element844

grow843:                                          ; preds = %store_element827
  %678 = icmp eq i64 %curr_cap840, 0
  %679 = mul i64 %curr_cap840, 2
  %new_cap845 = select i1 %678, i64 4, i64 %679
  %new_byte_count846 = mul i64 %new_cap845, 8
  %reallocated_data847 = call ptr @realloc(ptr %curr_data841, i64 %new_byte_count846)
  store i64 %new_cap845, ptr %cap_ptr837, align 8
  store ptr %reallocated_data847, ptr %data_ptr_ptr838, align 8
  br label %store_element844

store_element844:                                 ; preds = %grow843, %store_element827
  %final_data848 = load ptr, ptr %data_ptr_ptr838, align 8
  %offset849 = mul i64 %curr_len839, 8
  %raw_elem_ptr850 = getelementptr i8, ptr %final_data848, i64 %offset849
  store double %676, ptr %raw_elem_ptr850, align 8
  %new_len851 = add i64 %curr_len839, 1
  store i64 %new_len851, ptr %len_ptr836, align 8
  %680 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 18
  %681 = load double, ptr %680, align 8
  %col_ptr_ptr852 = getelementptr ptr, ptr %589, i64 18
  %682 = load ptr, ptr %col_ptr_ptr852, align 8
  %len_ptr853 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %682, i32 0, i32 0
  %cap_ptr854 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %682, i32 0, i32 1
  %data_ptr_ptr855 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %682, i32 0, i32 2
  %curr_len856 = load i64, ptr %len_ptr853, align 4
  %curr_cap857 = load i64, ptr %cap_ptr854, align 4
  %curr_data858 = load ptr, ptr %data_ptr_ptr855, align 8
  %needs_grow859 = icmp sge i64 %curr_len856, %curr_cap857
  br i1 %needs_grow859, label %grow860, label %store_element861

grow860:                                          ; preds = %store_element844
  %683 = icmp eq i64 %curr_cap857, 0
  %684 = mul i64 %curr_cap857, 2
  %new_cap862 = select i1 %683, i64 4, i64 %684
  %new_byte_count863 = mul i64 %new_cap862, 8
  %reallocated_data864 = call ptr @realloc(ptr %curr_data858, i64 %new_byte_count863)
  store i64 %new_cap862, ptr %cap_ptr854, align 8
  store ptr %reallocated_data864, ptr %data_ptr_ptr855, align 8
  br label %store_element861

store_element861:                                 ; preds = %grow860, %store_element844
  %final_data865 = load ptr, ptr %data_ptr_ptr855, align 8
  %offset866 = mul i64 %curr_len856, 8
  %raw_elem_ptr867 = getelementptr i8, ptr %final_data865, i64 %offset866
  store double %681, ptr %raw_elem_ptr867, align 8
  %new_len868 = add i64 %curr_len856, 1
  store i64 %new_len868, ptr %len_ptr853, align 8
  %685 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 19
  %686 = load double, ptr %685, align 8
  %col_ptr_ptr869 = getelementptr ptr, ptr %589, i64 19
  %687 = load ptr, ptr %col_ptr_ptr869, align 8
  %len_ptr870 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %687, i32 0, i32 0
  %cap_ptr871 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %687, i32 0, i32 1
  %data_ptr_ptr872 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %687, i32 0, i32 2
  %curr_len873 = load i64, ptr %len_ptr870, align 4
  %curr_cap874 = load i64, ptr %cap_ptr871, align 4
  %curr_data875 = load ptr, ptr %data_ptr_ptr872, align 8
  %needs_grow876 = icmp sge i64 %curr_len873, %curr_cap874
  br i1 %needs_grow876, label %grow877, label %store_element878

grow877:                                          ; preds = %store_element861
  %688 = icmp eq i64 %curr_cap874, 0
  %689 = mul i64 %curr_cap874, 2
  %new_cap879 = select i1 %688, i64 4, i64 %689
  %new_byte_count880 = mul i64 %new_cap879, 8
  %reallocated_data881 = call ptr @realloc(ptr %curr_data875, i64 %new_byte_count880)
  store i64 %new_cap879, ptr %cap_ptr871, align 8
  store ptr %reallocated_data881, ptr %data_ptr_ptr872, align 8
  br label %store_element878

store_element878:                                 ; preds = %grow877, %store_element861
  %final_data882 = load ptr, ptr %data_ptr_ptr872, align 8
  %offset883 = mul i64 %curr_len873, 8
  %raw_elem_ptr884 = getelementptr i8, ptr %final_data882, i64 %offset883
  store double %686, ptr %raw_elem_ptr884, align 8
  %new_len885 = add i64 %curr_len873, 1
  store i64 %new_len885, ptr %len_ptr870, align 8
  %690 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 20
  %691 = load i64, ptr %690, align 4
  %col_ptr_ptr886 = getelementptr ptr, ptr %589, i64 20
  %692 = load ptr, ptr %col_ptr_ptr886, align 8
  %len_ptr887 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %692, i32 0, i32 0
  %cap_ptr888 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %692, i32 0, i32 1
  %data_ptr_ptr889 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %692, i32 0, i32 2
  %curr_len890 = load i64, ptr %len_ptr887, align 4
  %curr_cap891 = load i64, ptr %cap_ptr888, align 4
  %curr_data892 = load ptr, ptr %data_ptr_ptr889, align 8
  %needs_grow893 = icmp sge i64 %curr_len890, %curr_cap891
  br i1 %needs_grow893, label %grow894, label %store_element895

grow894:                                          ; preds = %store_element878
  %693 = icmp eq i64 %curr_cap891, 0
  %694 = mul i64 %curr_cap891, 2
  %new_cap896 = select i1 %693, i64 4, i64 %694
  %new_byte_count897 = mul i64 %new_cap896, 8
  %reallocated_data898 = call ptr @realloc(ptr %curr_data892, i64 %new_byte_count897)
  store i64 %new_cap896, ptr %cap_ptr888, align 8
  store ptr %reallocated_data898, ptr %data_ptr_ptr889, align 8
  br label %store_element895

store_element895:                                 ; preds = %grow894, %store_element878
  %final_data899 = load ptr, ptr %data_ptr_ptr889, align 8
  %offset900 = mul i64 %curr_len890, 8
  %raw_elem_ptr901 = getelementptr i8, ptr %final_data899, i64 %offset900
  store i64 %691, ptr %raw_elem_ptr901, align 8
  %new_len902 = add i64 %curr_len890, 1
  store i64 %new_len902, ptr %len_ptr887, align 8
  %695 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 21
  %696 = load i8, ptr %695, align 1
  %col_ptr_ptr903 = getelementptr ptr, ptr %589, i64 21
  %697 = load ptr, ptr %col_ptr_ptr903, align 8
  %len_ptr904 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %697, i32 0, i32 0
  %cap_ptr905 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %697, i32 0, i32 1
  %data_ptr_ptr906 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %697, i32 0, i32 2
  %curr_len907 = load i64, ptr %len_ptr904, align 4
  %curr_cap908 = load i64, ptr %cap_ptr905, align 4
  %curr_data909 = load ptr, ptr %data_ptr_ptr906, align 8
  %needs_grow910 = icmp sge i64 %curr_len907, %curr_cap908
  br i1 %needs_grow910, label %grow911, label %store_element912

grow911:                                          ; preds = %store_element895
  %698 = icmp eq i64 %curr_cap908, 0
  %699 = mul i64 %curr_cap908, 2
  %new_cap913 = select i1 %698, i64 4, i64 %699
  %new_byte_count914 = mul i64 %new_cap913, 1
  %reallocated_data915 = call ptr @realloc(ptr %curr_data909, i64 %new_byte_count914)
  store i64 %new_cap913, ptr %cap_ptr905, align 8
  store ptr %reallocated_data915, ptr %data_ptr_ptr906, align 8
  br label %store_element912

store_element912:                                 ; preds = %grow911, %store_element895
  %final_data916 = load ptr, ptr %data_ptr_ptr906, align 8
  %offset917 = mul i64 %curr_len907, 1
  %raw_elem_ptr918 = getelementptr i8, ptr %final_data916, i64 %offset917
  store i8 %696, ptr %raw_elem_ptr918, align 8
  %new_len919 = add i64 %curr_len907, 1
  store i64 %new_len919, ptr %len_ptr904, align 8
  %700 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 22
  %701 = load i8, ptr %700, align 1
  %col_ptr_ptr920 = getelementptr ptr, ptr %589, i64 22
  %702 = load ptr, ptr %col_ptr_ptr920, align 8
  %len_ptr921 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %702, i32 0, i32 0
  %cap_ptr922 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %702, i32 0, i32 1
  %data_ptr_ptr923 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %702, i32 0, i32 2
  %curr_len924 = load i64, ptr %len_ptr921, align 4
  %curr_cap925 = load i64, ptr %cap_ptr922, align 4
  %curr_data926 = load ptr, ptr %data_ptr_ptr923, align 8
  %needs_grow927 = icmp sge i64 %curr_len924, %curr_cap925
  br i1 %needs_grow927, label %grow928, label %store_element929

grow928:                                          ; preds = %store_element912
  %703 = icmp eq i64 %curr_cap925, 0
  %704 = mul i64 %curr_cap925, 2
  %new_cap930 = select i1 %703, i64 4, i64 %704
  %new_byte_count931 = mul i64 %new_cap930, 1
  %reallocated_data932 = call ptr @realloc(ptr %curr_data926, i64 %new_byte_count931)
  store i64 %new_cap930, ptr %cap_ptr922, align 8
  store ptr %reallocated_data932, ptr %data_ptr_ptr923, align 8
  br label %store_element929

store_element929:                                 ; preds = %grow928, %store_element912
  %final_data933 = load ptr, ptr %data_ptr_ptr923, align 8
  %offset934 = mul i64 %curr_len924, 1
  %raw_elem_ptr935 = getelementptr i8, ptr %final_data933, i64 %offset934
  store i8 %701, ptr %raw_elem_ptr935, align 8
  %new_len936 = add i64 %curr_len924, 1
  store i64 %new_len936, ptr %len_ptr921, align 8
  %705 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 23
  %706 = load i8, ptr %705, align 1
  %col_ptr_ptr937 = getelementptr ptr, ptr %589, i64 23
  %707 = load ptr, ptr %col_ptr_ptr937, align 8
  %len_ptr938 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %707, i32 0, i32 0
  %cap_ptr939 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %707, i32 0, i32 1
  %data_ptr_ptr940 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %707, i32 0, i32 2
  %curr_len941 = load i64, ptr %len_ptr938, align 4
  %curr_cap942 = load i64, ptr %cap_ptr939, align 4
  %curr_data943 = load ptr, ptr %data_ptr_ptr940, align 8
  %needs_grow944 = icmp sge i64 %curr_len941, %curr_cap942
  br i1 %needs_grow944, label %grow945, label %store_element946

grow945:                                          ; preds = %store_element929
  %708 = icmp eq i64 %curr_cap942, 0
  %709 = mul i64 %curr_cap942, 2
  %new_cap947 = select i1 %708, i64 4, i64 %709
  %new_byte_count948 = mul i64 %new_cap947, 1
  %reallocated_data949 = call ptr @realloc(ptr %curr_data943, i64 %new_byte_count948)
  store i64 %new_cap947, ptr %cap_ptr939, align 8
  store ptr %reallocated_data949, ptr %data_ptr_ptr940, align 8
  br label %store_element946

store_element946:                                 ; preds = %grow945, %store_element929
  %final_data950 = load ptr, ptr %data_ptr_ptr940, align 8
  %offset951 = mul i64 %curr_len941, 1
  %raw_elem_ptr952 = getelementptr i8, ptr %final_data950, i64 %offset951
  store i8 %706, ptr %raw_elem_ptr952, align 8
  %new_len953 = add i64 %curr_len941, 1
  store i64 %new_len953, ptr %len_ptr938, align 8
  %710 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 24
  %711 = load i8, ptr %710, align 1
  %col_ptr_ptr954 = getelementptr ptr, ptr %589, i64 24
  %712 = load ptr, ptr %col_ptr_ptr954, align 8
  %len_ptr955 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %712, i32 0, i32 0
  %cap_ptr956 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %712, i32 0, i32 1
  %data_ptr_ptr957 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %712, i32 0, i32 2
  %curr_len958 = load i64, ptr %len_ptr955, align 4
  %curr_cap959 = load i64, ptr %cap_ptr956, align 4
  %curr_data960 = load ptr, ptr %data_ptr_ptr957, align 8
  %needs_grow961 = icmp sge i64 %curr_len958, %curr_cap959
  br i1 %needs_grow961, label %grow962, label %store_element963

grow962:                                          ; preds = %store_element946
  %713 = icmp eq i64 %curr_cap959, 0
  %714 = mul i64 %curr_cap959, 2
  %new_cap964 = select i1 %713, i64 4, i64 %714
  %new_byte_count965 = mul i64 %new_cap964, 1
  %reallocated_data966 = call ptr @realloc(ptr %curr_data960, i64 %new_byte_count965)
  store i64 %new_cap964, ptr %cap_ptr956, align 8
  store ptr %reallocated_data966, ptr %data_ptr_ptr957, align 8
  br label %store_element963

store_element963:                                 ; preds = %grow962, %store_element946
  %final_data967 = load ptr, ptr %data_ptr_ptr957, align 8
  %offset968 = mul i64 %curr_len958, 1
  %raw_elem_ptr969 = getelementptr i8, ptr %final_data967, i64 %offset968
  store i8 %711, ptr %raw_elem_ptr969, align 8
  %new_len970 = add i64 %curr_len958, 1
  store i64 %new_len970, ptr %len_ptr955, align 8
  %715 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 25
  %716 = load i8, ptr %715, align 1
  %col_ptr_ptr971 = getelementptr ptr, ptr %589, i64 25
  %717 = load ptr, ptr %col_ptr_ptr971, align 8
  %len_ptr972 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %717, i32 0, i32 0
  %cap_ptr973 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %717, i32 0, i32 1
  %data_ptr_ptr974 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %717, i32 0, i32 2
  %curr_len975 = load i64, ptr %len_ptr972, align 4
  %curr_cap976 = load i64, ptr %cap_ptr973, align 4
  %curr_data977 = load ptr, ptr %data_ptr_ptr974, align 8
  %needs_grow978 = icmp sge i64 %curr_len975, %curr_cap976
  br i1 %needs_grow978, label %grow979, label %store_element980

grow979:                                          ; preds = %store_element963
  %718 = icmp eq i64 %curr_cap976, 0
  %719 = mul i64 %curr_cap976, 2
  %new_cap981 = select i1 %718, i64 4, i64 %719
  %new_byte_count982 = mul i64 %new_cap981, 1
  %reallocated_data983 = call ptr @realloc(ptr %curr_data977, i64 %new_byte_count982)
  store i64 %new_cap981, ptr %cap_ptr973, align 8
  store ptr %reallocated_data983, ptr %data_ptr_ptr974, align 8
  br label %store_element980

store_element980:                                 ; preds = %grow979, %store_element963
  %final_data984 = load ptr, ptr %data_ptr_ptr974, align 8
  %offset985 = mul i64 %curr_len975, 1
  %raw_elem_ptr986 = getelementptr i8, ptr %final_data984, i64 %offset985
  store i8 %716, ptr %raw_elem_ptr986, align 8
  %new_len987 = add i64 %curr_len975, 1
  store i64 %new_len987, ptr %len_ptr972, align 8
  %720 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 26
  %721 = load i8, ptr %720, align 1
  %col_ptr_ptr988 = getelementptr ptr, ptr %589, i64 26
  %722 = load ptr, ptr %col_ptr_ptr988, align 8
  %len_ptr989 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %722, i32 0, i32 0
  %cap_ptr990 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %722, i32 0, i32 1
  %data_ptr_ptr991 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %722, i32 0, i32 2
  %curr_len992 = load i64, ptr %len_ptr989, align 4
  %curr_cap993 = load i64, ptr %cap_ptr990, align 4
  %curr_data994 = load ptr, ptr %data_ptr_ptr991, align 8
  %needs_grow995 = icmp sge i64 %curr_len992, %curr_cap993
  br i1 %needs_grow995, label %grow996, label %store_element997

grow996:                                          ; preds = %store_element980
  %723 = icmp eq i64 %curr_cap993, 0
  %724 = mul i64 %curr_cap993, 2
  %new_cap998 = select i1 %723, i64 4, i64 %724
  %new_byte_count999 = mul i64 %new_cap998, 1
  %reallocated_data1000 = call ptr @realloc(ptr %curr_data994, i64 %new_byte_count999)
  store i64 %new_cap998, ptr %cap_ptr990, align 8
  store ptr %reallocated_data1000, ptr %data_ptr_ptr991, align 8
  br label %store_element997

store_element997:                                 ; preds = %grow996, %store_element980
  %final_data1001 = load ptr, ptr %data_ptr_ptr991, align 8
  %offset1002 = mul i64 %curr_len992, 1
  %raw_elem_ptr1003 = getelementptr i8, ptr %final_data1001, i64 %offset1002
  store i8 %721, ptr %raw_elem_ptr1003, align 8
  %new_len1004 = add i64 %curr_len992, 1
  store i64 %new_len1004, ptr %len_ptr989, align 8
  %725 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 27
  %726 = load i8, ptr %725, align 1
  %col_ptr_ptr1005 = getelementptr ptr, ptr %589, i64 27
  %727 = load ptr, ptr %col_ptr_ptr1005, align 8
  %len_ptr1006 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %727, i32 0, i32 0
  %cap_ptr1007 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %727, i32 0, i32 1
  %data_ptr_ptr1008 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %727, i32 0, i32 2
  %curr_len1009 = load i64, ptr %len_ptr1006, align 4
  %curr_cap1010 = load i64, ptr %cap_ptr1007, align 4
  %curr_data1011 = load ptr, ptr %data_ptr_ptr1008, align 8
  %needs_grow1012 = icmp sge i64 %curr_len1009, %curr_cap1010
  br i1 %needs_grow1012, label %grow1013, label %store_element1014

grow1013:                                         ; preds = %store_element997
  %728 = icmp eq i64 %curr_cap1010, 0
  %729 = mul i64 %curr_cap1010, 2
  %new_cap1015 = select i1 %728, i64 4, i64 %729
  %new_byte_count1016 = mul i64 %new_cap1015, 1
  %reallocated_data1017 = call ptr @realloc(ptr %curr_data1011, i64 %new_byte_count1016)
  store i64 %new_cap1015, ptr %cap_ptr1007, align 8
  store ptr %reallocated_data1017, ptr %data_ptr_ptr1008, align 8
  br label %store_element1014

store_element1014:                                ; preds = %grow1013, %store_element997
  %final_data1018 = load ptr, ptr %data_ptr_ptr1008, align 8
  %offset1019 = mul i64 %curr_len1009, 1
  %raw_elem_ptr1020 = getelementptr i8, ptr %final_data1018, i64 %offset1019
  store i8 %726, ptr %raw_elem_ptr1020, align 8
  %new_len1021 = add i64 %curr_len1009, 1
  store i64 %new_len1021, ptr %len_ptr1006, align 8
  %730 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 28
  %731 = load i8, ptr %730, align 1
  %col_ptr_ptr1022 = getelementptr ptr, ptr %589, i64 28
  %732 = load ptr, ptr %col_ptr_ptr1022, align 8
  %len_ptr1023 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %732, i32 0, i32 0
  %cap_ptr1024 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %732, i32 0, i32 1
  %data_ptr_ptr1025 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %732, i32 0, i32 2
  %curr_len1026 = load i64, ptr %len_ptr1023, align 4
  %curr_cap1027 = load i64, ptr %cap_ptr1024, align 4
  %curr_data1028 = load ptr, ptr %data_ptr_ptr1025, align 8
  %needs_grow1029 = icmp sge i64 %curr_len1026, %curr_cap1027
  br i1 %needs_grow1029, label %grow1030, label %store_element1031

grow1030:                                         ; preds = %store_element1014
  %733 = icmp eq i64 %curr_cap1027, 0
  %734 = mul i64 %curr_cap1027, 2
  %new_cap1032 = select i1 %733, i64 4, i64 %734
  %new_byte_count1033 = mul i64 %new_cap1032, 1
  %reallocated_data1034 = call ptr @realloc(ptr %curr_data1028, i64 %new_byte_count1033)
  store i64 %new_cap1032, ptr %cap_ptr1024, align 8
  store ptr %reallocated_data1034, ptr %data_ptr_ptr1025, align 8
  br label %store_element1031

store_element1031:                                ; preds = %grow1030, %store_element1014
  %final_data1035 = load ptr, ptr %data_ptr_ptr1025, align 8
  %offset1036 = mul i64 %curr_len1026, 1
  %raw_elem_ptr1037 = getelementptr i8, ptr %final_data1035, i64 %offset1036
  store i8 %731, ptr %raw_elem_ptr1037, align 8
  %new_len1038 = add i64 %curr_len1026, 1
  store i64 %new_len1038, ptr %len_ptr1023, align 8
  %735 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 29
  %736 = load i8, ptr %735, align 1
  %col_ptr_ptr1039 = getelementptr ptr, ptr %589, i64 29
  %737 = load ptr, ptr %col_ptr_ptr1039, align 8
  %len_ptr1040 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %737, i32 0, i32 0
  %cap_ptr1041 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %737, i32 0, i32 1
  %data_ptr_ptr1042 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %737, i32 0, i32 2
  %curr_len1043 = load i64, ptr %len_ptr1040, align 4
  %curr_cap1044 = load i64, ptr %cap_ptr1041, align 4
  %curr_data1045 = load ptr, ptr %data_ptr_ptr1042, align 8
  %needs_grow1046 = icmp sge i64 %curr_len1043, %curr_cap1044
  br i1 %needs_grow1046, label %grow1047, label %store_element1048

grow1047:                                         ; preds = %store_element1031
  %738 = icmp eq i64 %curr_cap1044, 0
  %739 = mul i64 %curr_cap1044, 2
  %new_cap1049 = select i1 %738, i64 4, i64 %739
  %new_byte_count1050 = mul i64 %new_cap1049, 1
  %reallocated_data1051 = call ptr @realloc(ptr %curr_data1045, i64 %new_byte_count1050)
  store i64 %new_cap1049, ptr %cap_ptr1041, align 8
  store ptr %reallocated_data1051, ptr %data_ptr_ptr1042, align 8
  br label %store_element1048

store_element1048:                                ; preds = %grow1047, %store_element1031
  %final_data1052 = load ptr, ptr %data_ptr_ptr1042, align 8
  %offset1053 = mul i64 %curr_len1043, 1
  %raw_elem_ptr1054 = getelementptr i8, ptr %final_data1052, i64 %offset1053
  store i8 %736, ptr %raw_elem_ptr1054, align 8
  %new_len1055 = add i64 %curr_len1043, 1
  store i64 %new_len1055, ptr %len_ptr1040, align 8
  %740 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 30
  %741 = load i8, ptr %740, align 1
  %col_ptr_ptr1056 = getelementptr ptr, ptr %589, i64 30
  %742 = load ptr, ptr %col_ptr_ptr1056, align 8
  %len_ptr1057 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %742, i32 0, i32 0
  %cap_ptr1058 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %742, i32 0, i32 1
  %data_ptr_ptr1059 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %742, i32 0, i32 2
  %curr_len1060 = load i64, ptr %len_ptr1057, align 4
  %curr_cap1061 = load i64, ptr %cap_ptr1058, align 4
  %curr_data1062 = load ptr, ptr %data_ptr_ptr1059, align 8
  %needs_grow1063 = icmp sge i64 %curr_len1060, %curr_cap1061
  br i1 %needs_grow1063, label %grow1064, label %store_element1065

grow1064:                                         ; preds = %store_element1048
  %743 = icmp eq i64 %curr_cap1061, 0
  %744 = mul i64 %curr_cap1061, 2
  %new_cap1066 = select i1 %743, i64 4, i64 %744
  %new_byte_count1067 = mul i64 %new_cap1066, 1
  %reallocated_data1068 = call ptr @realloc(ptr %curr_data1062, i64 %new_byte_count1067)
  store i64 %new_cap1066, ptr %cap_ptr1058, align 8
  store ptr %reallocated_data1068, ptr %data_ptr_ptr1059, align 8
  br label %store_element1065

store_element1065:                                ; preds = %grow1064, %store_element1048
  %final_data1069 = load ptr, ptr %data_ptr_ptr1059, align 8
  %offset1070 = mul i64 %curr_len1060, 1
  %raw_elem_ptr1071 = getelementptr i8, ptr %final_data1069, i64 %offset1070
  store i8 %741, ptr %raw_elem_ptr1071, align 8
  %new_len1072 = add i64 %curr_len1060, 1
  store i64 %new_len1072, ptr %len_ptr1057, align 8
  %745 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 31
  %746 = load i8, ptr %745, align 1
  %col_ptr_ptr1073 = getelementptr ptr, ptr %589, i64 31
  %747 = load ptr, ptr %col_ptr_ptr1073, align 8
  %len_ptr1074 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %747, i32 0, i32 0
  %cap_ptr1075 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %747, i32 0, i32 1
  %data_ptr_ptr1076 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %747, i32 0, i32 2
  %curr_len1077 = load i64, ptr %len_ptr1074, align 4
  %curr_cap1078 = load i64, ptr %cap_ptr1075, align 4
  %curr_data1079 = load ptr, ptr %data_ptr_ptr1076, align 8
  %needs_grow1080 = icmp sge i64 %curr_len1077, %curr_cap1078
  br i1 %needs_grow1080, label %grow1081, label %store_element1082

grow1081:                                         ; preds = %store_element1065
  %748 = icmp eq i64 %curr_cap1078, 0
  %749 = mul i64 %curr_cap1078, 2
  %new_cap1083 = select i1 %748, i64 4, i64 %749
  %new_byte_count1084 = mul i64 %new_cap1083, 1
  %reallocated_data1085 = call ptr @realloc(ptr %curr_data1079, i64 %new_byte_count1084)
  store i64 %new_cap1083, ptr %cap_ptr1075, align 8
  store ptr %reallocated_data1085, ptr %data_ptr_ptr1076, align 8
  br label %store_element1082

store_element1082:                                ; preds = %grow1081, %store_element1065
  %final_data1086 = load ptr, ptr %data_ptr_ptr1076, align 8
  %offset1087 = mul i64 %curr_len1077, 1
  %raw_elem_ptr1088 = getelementptr i8, ptr %final_data1086, i64 %offset1087
  store i8 %746, ptr %raw_elem_ptr1088, align 8
  %new_len1089 = add i64 %curr_len1077, 1
  store i64 %new_len1089, ptr %len_ptr1074, align 8
  %750 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 32
  %751 = load i8, ptr %750, align 1
  %col_ptr_ptr1090 = getelementptr ptr, ptr %589, i64 32
  %752 = load ptr, ptr %col_ptr_ptr1090, align 8
  %len_ptr1091 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %752, i32 0, i32 0
  %cap_ptr1092 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %752, i32 0, i32 1
  %data_ptr_ptr1093 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %752, i32 0, i32 2
  %curr_len1094 = load i64, ptr %len_ptr1091, align 4
  %curr_cap1095 = load i64, ptr %cap_ptr1092, align 4
  %curr_data1096 = load ptr, ptr %data_ptr_ptr1093, align 8
  %needs_grow1097 = icmp sge i64 %curr_len1094, %curr_cap1095
  br i1 %needs_grow1097, label %grow1098, label %store_element1099

grow1098:                                         ; preds = %store_element1082
  %753 = icmp eq i64 %curr_cap1095, 0
  %754 = mul i64 %curr_cap1095, 2
  %new_cap1100 = select i1 %753, i64 4, i64 %754
  %new_byte_count1101 = mul i64 %new_cap1100, 1
  %reallocated_data1102 = call ptr @realloc(ptr %curr_data1096, i64 %new_byte_count1101)
  store i64 %new_cap1100, ptr %cap_ptr1092, align 8
  store ptr %reallocated_data1102, ptr %data_ptr_ptr1093, align 8
  br label %store_element1099

store_element1099:                                ; preds = %grow1098, %store_element1082
  %final_data1103 = load ptr, ptr %data_ptr_ptr1093, align 8
  %offset1104 = mul i64 %curr_len1094, 1
  %raw_elem_ptr1105 = getelementptr i8, ptr %final_data1103, i64 %offset1104
  store i8 %751, ptr %raw_elem_ptr1105, align 8
  %new_len1106 = add i64 %curr_len1094, 1
  store i64 %new_len1106, ptr %len_ptr1091, align 8
  %755 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 33
  %756 = load i8, ptr %755, align 1
  %col_ptr_ptr1107 = getelementptr ptr, ptr %589, i64 33
  %757 = load ptr, ptr %col_ptr_ptr1107, align 8
  %len_ptr1108 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %757, i32 0, i32 0
  %cap_ptr1109 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %757, i32 0, i32 1
  %data_ptr_ptr1110 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %757, i32 0, i32 2
  %curr_len1111 = load i64, ptr %len_ptr1108, align 4
  %curr_cap1112 = load i64, ptr %cap_ptr1109, align 4
  %curr_data1113 = load ptr, ptr %data_ptr_ptr1110, align 8
  %needs_grow1114 = icmp sge i64 %curr_len1111, %curr_cap1112
  br i1 %needs_grow1114, label %grow1115, label %store_element1116

grow1115:                                         ; preds = %store_element1099
  %758 = icmp eq i64 %curr_cap1112, 0
  %759 = mul i64 %curr_cap1112, 2
  %new_cap1117 = select i1 %758, i64 4, i64 %759
  %new_byte_count1118 = mul i64 %new_cap1117, 1
  %reallocated_data1119 = call ptr @realloc(ptr %curr_data1113, i64 %new_byte_count1118)
  store i64 %new_cap1117, ptr %cap_ptr1109, align 8
  store ptr %reallocated_data1119, ptr %data_ptr_ptr1110, align 8
  br label %store_element1116

store_element1116:                                ; preds = %grow1115, %store_element1099
  %final_data1120 = load ptr, ptr %data_ptr_ptr1110, align 8
  %offset1121 = mul i64 %curr_len1111, 1
  %raw_elem_ptr1122 = getelementptr i8, ptr %final_data1120, i64 %offset1121
  store i8 %756, ptr %raw_elem_ptr1122, align 8
  %new_len1123 = add i64 %curr_len1111, 1
  store i64 %new_len1123, ptr %len_ptr1108, align 8
  %760 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 34
  %761 = load i8, ptr %760, align 1
  %col_ptr_ptr1124 = getelementptr ptr, ptr %589, i64 34
  %762 = load ptr, ptr %col_ptr_ptr1124, align 8
  %len_ptr1125 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %762, i32 0, i32 0
  %cap_ptr1126 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %762, i32 0, i32 1
  %data_ptr_ptr1127 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %762, i32 0, i32 2
  %curr_len1128 = load i64, ptr %len_ptr1125, align 4
  %curr_cap1129 = load i64, ptr %cap_ptr1126, align 4
  %curr_data1130 = load ptr, ptr %data_ptr_ptr1127, align 8
  %needs_grow1131 = icmp sge i64 %curr_len1128, %curr_cap1129
  br i1 %needs_grow1131, label %grow1132, label %store_element1133

grow1132:                                         ; preds = %store_element1116
  %763 = icmp eq i64 %curr_cap1129, 0
  %764 = mul i64 %curr_cap1129, 2
  %new_cap1134 = select i1 %763, i64 4, i64 %764
  %new_byte_count1135 = mul i64 %new_cap1134, 1
  %reallocated_data1136 = call ptr @realloc(ptr %curr_data1130, i64 %new_byte_count1135)
  store i64 %new_cap1134, ptr %cap_ptr1126, align 8
  store ptr %reallocated_data1136, ptr %data_ptr_ptr1127, align 8
  br label %store_element1133

store_element1133:                                ; preds = %grow1132, %store_element1116
  %final_data1137 = load ptr, ptr %data_ptr_ptr1127, align 8
  %offset1138 = mul i64 %curr_len1128, 1
  %raw_elem_ptr1139 = getelementptr i8, ptr %final_data1137, i64 %offset1138
  store i8 %761, ptr %raw_elem_ptr1139, align 8
  %new_len1140 = add i64 %curr_len1128, 1
  store i64 %new_len1140, ptr %len_ptr1125, align 8
  %765 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 35
  %766 = load i8, ptr %765, align 1
  %col_ptr_ptr1141 = getelementptr ptr, ptr %589, i64 35
  %767 = load ptr, ptr %col_ptr_ptr1141, align 8
  %len_ptr1142 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %767, i32 0, i32 0
  %cap_ptr1143 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %767, i32 0, i32 1
  %data_ptr_ptr1144 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %767, i32 0, i32 2
  %curr_len1145 = load i64, ptr %len_ptr1142, align 4
  %curr_cap1146 = load i64, ptr %cap_ptr1143, align 4
  %curr_data1147 = load ptr, ptr %data_ptr_ptr1144, align 8
  %needs_grow1148 = icmp sge i64 %curr_len1145, %curr_cap1146
  br i1 %needs_grow1148, label %grow1149, label %store_element1150

grow1149:                                         ; preds = %store_element1133
  %768 = icmp eq i64 %curr_cap1146, 0
  %769 = mul i64 %curr_cap1146, 2
  %new_cap1151 = select i1 %768, i64 4, i64 %769
  %new_byte_count1152 = mul i64 %new_cap1151, 1
  %reallocated_data1153 = call ptr @realloc(ptr %curr_data1147, i64 %new_byte_count1152)
  store i64 %new_cap1151, ptr %cap_ptr1143, align 8
  store ptr %reallocated_data1153, ptr %data_ptr_ptr1144, align 8
  br label %store_element1150

store_element1150:                                ; preds = %grow1149, %store_element1133
  %final_data1154 = load ptr, ptr %data_ptr_ptr1144, align 8
  %offset1155 = mul i64 %curr_len1145, 1
  %raw_elem_ptr1156 = getelementptr i8, ptr %final_data1154, i64 %offset1155
  store i8 %766, ptr %raw_elem_ptr1156, align 8
  %new_len1157 = add i64 %curr_len1145, 1
  store i64 %new_len1157, ptr %len_ptr1142, align 8
  %770 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record_mem, i32 0, i32 36
  %771 = load i8, ptr %770, align 1
  %col_ptr_ptr1158 = getelementptr ptr, ptr %589, i64 36
  %772 = load ptr, ptr %col_ptr_ptr1158, align 8
  %len_ptr1159 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %772, i32 0, i32 0
  %cap_ptr1160 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %772, i32 0, i32 1
  %data_ptr_ptr1161 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %772, i32 0, i32 2
  %curr_len1162 = load i64, ptr %len_ptr1159, align 4
  %curr_cap1163 = load i64, ptr %cap_ptr1160, align 4
  %curr_data1164 = load ptr, ptr %data_ptr_ptr1161, align 8
  %needs_grow1165 = icmp sge i64 %curr_len1162, %curr_cap1163
  br i1 %needs_grow1165, label %grow1166, label %store_element1167

grow1166:                                         ; preds = %store_element1150
  %773 = icmp eq i64 %curr_cap1163, 0
  %774 = mul i64 %curr_cap1163, 2
  %new_cap1168 = select i1 %773, i64 4, i64 %774
  %new_byte_count1169 = mul i64 %new_cap1168, 1
  %reallocated_data1170 = call ptr @realloc(ptr %curr_data1164, i64 %new_byte_count1169)
  store i64 %new_cap1168, ptr %cap_ptr1160, align 8
  store ptr %reallocated_data1170, ptr %data_ptr_ptr1161, align 8
  br label %store_element1167

store_element1167:                                ; preds = %grow1166, %store_element1150
  %final_data1171 = load ptr, ptr %data_ptr_ptr1161, align 8
  %offset1172 = mul i64 %curr_len1162, 1
  %raw_elem_ptr1173 = getelementptr i8, ptr %final_data1171, i64 %offset1172
  store i8 %771, ptr %raw_elem_ptr1173, align 8
  %new_len1174 = add i64 %curr_len1162, 1
  store i64 %new_len1174, ptr %len_ptr1159, align 8
  %775 = getelementptr inbounds nuw %dataframe, ptr %df_cast, i32 0, i32 3
  %776 = load i64, ptr %775, align 4
  %777 = add i64 %776, 1
  store i64 %777, ptr %775, align 8
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
