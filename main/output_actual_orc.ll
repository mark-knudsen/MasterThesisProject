; ModuleID = 'repl_module'
source_filename = "repl_module"

@str = private unnamed_addr constant [50 x i8] c"CSV/Fire_Prediction_2023_Bolivia_encoded_FULL.csv\00", align 1
@csv_schema_code = private unnamed_addr constant [38 x i8] c"SFFFFFFFFFFFFFFFFFFFIBBBBBBBBBBBBBBBB\00", align 1
@col_name_0 = private unnamed_addr constant [5 x i8] c"date\00", align 1
@col_name_1 = private unnamed_addr constant [9 x i8] c"latitude\00", align 1
@col_name_2 = private unnamed_addr constant [10 x i8] c"longitude\00", align 1
@col_name_3 = private unnamed_addr constant [15 x i8] c"wind-speed-min\00", align 1
@col_name_4 = private unnamed_addr constant [15 x i8] c"wind-speed-max\00", align 1
@col_name_5 = private unnamed_addr constant [16 x i8] c"wind-speed-mean\00", align 1
@col_name_6 = private unnamed_addr constant [19 x i8] c"wind-direction-min\00", align 1
@col_name_7 = private unnamed_addr constant [19 x i8] c"wind-direction-max\00", align 1
@col_name_8 = private unnamed_addr constant [20 x i8] c"wind-direction-mean\00", align 1
@col_name_9 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-min\00", align 1
@col_name_10 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-max\00", align 1
@col_name_11 = private unnamed_addr constant [29 x i8] c"surface-air-temperature-mean\00", align 1
@col_name_12 = private unnamed_addr constant [19 x i8] c"total-rainfall-sum\00", align 1
@col_name_13 = private unnamed_addr constant [21 x i8] c"surface-humidity-min\00", align 1
@col_name_14 = private unnamed_addr constant [21 x i8] c"surface-humidity-max\00", align 1
@col_name_15 = private unnamed_addr constant [22 x i8] c"surface-humidity-mean\00", align 1
@col_name_16 = private unnamed_addr constant [5 x i8] c"ndvi\00", align 1
@col_name_17 = private unnamed_addr constant [10 x i8] c"elevation\00", align 1
@col_name_18 = private unnamed_addr constant [6 x i8] c"slope\00", align 1
@col_name_19 = private unnamed_addr constant [7 x i8] c"aspect\00", align 1
@col_name_20 = private unnamed_addr constant [11 x i8] c"fire_label\00", align 1
@col_name_21 = private unnamed_addr constant [19 x i8] c"land_cover_class_1\00", align 1
@col_name_22 = private unnamed_addr constant [19 x i8] c"land_cover_class_2\00", align 1
@col_name_23 = private unnamed_addr constant [19 x i8] c"land_cover_class_4\00", align 1
@col_name_24 = private unnamed_addr constant [19 x i8] c"land_cover_class_5\00", align 1
@col_name_25 = private unnamed_addr constant [19 x i8] c"land_cover_class_6\00", align 1
@col_name_26 = private unnamed_addr constant [19 x i8] c"land_cover_class_7\00", align 1
@col_name_27 = private unnamed_addr constant [19 x i8] c"land_cover_class_8\00", align 1
@col_name_28 = private unnamed_addr constant [19 x i8] c"land_cover_class_9\00", align 1
@col_name_29 = private unnamed_addr constant [20 x i8] c"land_cover_class_10\00", align 1
@col_name_30 = private unnamed_addr constant [20 x i8] c"land_cover_class_11\00", align 1
@col_name_31 = private unnamed_addr constant [20 x i8] c"land_cover_class_12\00", align 1
@col_name_32 = private unnamed_addr constant [20 x i8] c"land_cover_class_13\00", align 1
@col_name_33 = private unnamed_addr constant [20 x i8] c"land_cover_class_14\00", align 1
@col_name_34 = private unnamed_addr constant [20 x i8] c"land_cover_class_15\00", align 1
@col_name_35 = private unnamed_addr constant [20 x i8] c"land_cover_class_16\00", align 1
@col_name_36 = private unnamed_addr constant [20 x i8] c"land_cover_class_17\00", align 1
@df = external global ptr, align 8

define ptr @main_3() {
entry:
  %csv_boxed_res = call ptr @ReadCsvInternal(ptr @str, ptr @csv_schema_code)
  %unbox_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %csv_boxed_res, i32 0, i32 1
  %raw_rows_ptr = load ptr, ptr %unbox_ptr, align 8
  %names_header = call ptr @malloc(i64 24)
  %names_data = call ptr @malloc(i64 296)
  %ptr_0 = getelementptr ptr, ptr %names_data, i64 0
  store ptr @col_name_0, ptr %ptr_0, align 8
  %ptr_1 = getelementptr ptr, ptr %names_data, i64 1
  store ptr @col_name_1, ptr %ptr_1, align 8
  %ptr_2 = getelementptr ptr, ptr %names_data, i64 2
  store ptr @col_name_2, ptr %ptr_2, align 8
  %ptr_3 = getelementptr ptr, ptr %names_data, i64 3
  store ptr @col_name_3, ptr %ptr_3, align 8
  %ptr_4 = getelementptr ptr, ptr %names_data, i64 4
  store ptr @col_name_4, ptr %ptr_4, align 8
  %ptr_5 = getelementptr ptr, ptr %names_data, i64 5
  store ptr @col_name_5, ptr %ptr_5, align 8
  %ptr_6 = getelementptr ptr, ptr %names_data, i64 6
  store ptr @col_name_6, ptr %ptr_6, align 8
  %ptr_7 = getelementptr ptr, ptr %names_data, i64 7
  store ptr @col_name_7, ptr %ptr_7, align 8
  %ptr_8 = getelementptr ptr, ptr %names_data, i64 8
  store ptr @col_name_8, ptr %ptr_8, align 8
  %ptr_9 = getelementptr ptr, ptr %names_data, i64 9
  store ptr @col_name_9, ptr %ptr_9, align 8
  %ptr_10 = getelementptr ptr, ptr %names_data, i64 10
  store ptr @col_name_10, ptr %ptr_10, align 8
  %ptr_11 = getelementptr ptr, ptr %names_data, i64 11
  store ptr @col_name_11, ptr %ptr_11, align 8
  %ptr_12 = getelementptr ptr, ptr %names_data, i64 12
  store ptr @col_name_12, ptr %ptr_12, align 8
  %ptr_13 = getelementptr ptr, ptr %names_data, i64 13
  store ptr @col_name_13, ptr %ptr_13, align 8
  %ptr_14 = getelementptr ptr, ptr %names_data, i64 14
  store ptr @col_name_14, ptr %ptr_14, align 8
  %ptr_15 = getelementptr ptr, ptr %names_data, i64 15
  store ptr @col_name_15, ptr %ptr_15, align 8
  %ptr_16 = getelementptr ptr, ptr %names_data, i64 16
  store ptr @col_name_16, ptr %ptr_16, align 8
  %ptr_17 = getelementptr ptr, ptr %names_data, i64 17
  store ptr @col_name_17, ptr %ptr_17, align 8
  %ptr_18 = getelementptr ptr, ptr %names_data, i64 18
  store ptr @col_name_18, ptr %ptr_18, align 8
  %ptr_19 = getelementptr ptr, ptr %names_data, i64 19
  store ptr @col_name_19, ptr %ptr_19, align 8
  %ptr_20 = getelementptr ptr, ptr %names_data, i64 20
  store ptr @col_name_20, ptr %ptr_20, align 8
  %ptr_21 = getelementptr ptr, ptr %names_data, i64 21
  store ptr @col_name_21, ptr %ptr_21, align 8
  %ptr_22 = getelementptr ptr, ptr %names_data, i64 22
  store ptr @col_name_22, ptr %ptr_22, align 8
  %ptr_23 = getelementptr ptr, ptr %names_data, i64 23
  store ptr @col_name_23, ptr %ptr_23, align 8
  %ptr_24 = getelementptr ptr, ptr %names_data, i64 24
  store ptr @col_name_24, ptr %ptr_24, align 8
  %ptr_25 = getelementptr ptr, ptr %names_data, i64 25
  store ptr @col_name_25, ptr %ptr_25, align 8
  %ptr_26 = getelementptr ptr, ptr %names_data, i64 26
  store ptr @col_name_26, ptr %ptr_26, align 8
  %ptr_27 = getelementptr ptr, ptr %names_data, i64 27
  store ptr @col_name_27, ptr %ptr_27, align 8
  %ptr_28 = getelementptr ptr, ptr %names_data, i64 28
  store ptr @col_name_28, ptr %ptr_28, align 8
  %ptr_29 = getelementptr ptr, ptr %names_data, i64 29
  store ptr @col_name_29, ptr %ptr_29, align 8
  %ptr_30 = getelementptr ptr, ptr %names_data, i64 30
  store ptr @col_name_30, ptr %ptr_30, align 8
  %ptr_31 = getelementptr ptr, ptr %names_data, i64 31
  store ptr @col_name_31, ptr %ptr_31, align 8
  %ptr_32 = getelementptr ptr, ptr %names_data, i64 32
  store ptr @col_name_32, ptr %ptr_32, align 8
  %ptr_33 = getelementptr ptr, ptr %names_data, i64 33
  store ptr @col_name_33, ptr %ptr_33, align 8
  %ptr_34 = getelementptr ptr, ptr %names_data, i64 34
  store ptr @col_name_34, ptr %ptr_34, align 8
  %ptr_35 = getelementptr ptr, ptr %names_data, i64 35
  store ptr @col_name_35, ptr %ptr_35, align 8
  %ptr_36 = getelementptr ptr, ptr %names_data, i64 36
  store ptr @col_name_36, ptr %ptr_36, align 8
  %len = getelementptr inbounds nuw { i64, i64, ptr }, ptr %names_header, i32 0, i32 0
  store i64 37, ptr %len, align 4
  %cap = getelementptr inbounds nuw { i64, i64, ptr }, ptr %names_header, i32 0, i32 1
  store i64 37, ptr %cap, align 4
  %data = getelementptr inbounds nuw { i64, i64, ptr }, ptr %names_header, i32 0, i32 2
  store ptr %names_data, ptr %data, align 8
  %types_header = call ptr @malloc(i64 24)
  %types_data = call ptr @malloc(i64 296)
  %ptr = getelementptr ptr, ptr %types_data, i64 0
  store ptr inttoptr (i64 4 to ptr), ptr %ptr, align 8
  %ptr1 = getelementptr ptr, ptr %types_data, i64 1
  store ptr inttoptr (i64 2 to ptr), ptr %ptr1, align 8
  %ptr2 = getelementptr ptr, ptr %types_data, i64 2
  store ptr inttoptr (i64 2 to ptr), ptr %ptr2, align 8
  %ptr3 = getelementptr ptr, ptr %types_data, i64 3
  store ptr inttoptr (i64 2 to ptr), ptr %ptr3, align 8
  %ptr4 = getelementptr ptr, ptr %types_data, i64 4
  store ptr inttoptr (i64 2 to ptr), ptr %ptr4, align 8
  %ptr5 = getelementptr ptr, ptr %types_data, i64 5
  store ptr inttoptr (i64 2 to ptr), ptr %ptr5, align 8
  %ptr6 = getelementptr ptr, ptr %types_data, i64 6
  store ptr inttoptr (i64 2 to ptr), ptr %ptr6, align 8
  %ptr7 = getelementptr ptr, ptr %types_data, i64 7
  store ptr inttoptr (i64 2 to ptr), ptr %ptr7, align 8
  %ptr8 = getelementptr ptr, ptr %types_data, i64 8
  store ptr inttoptr (i64 2 to ptr), ptr %ptr8, align 8
  %ptr9 = getelementptr ptr, ptr %types_data, i64 9
  store ptr inttoptr (i64 2 to ptr), ptr %ptr9, align 8
  %ptr10 = getelementptr ptr, ptr %types_data, i64 10
  store ptr inttoptr (i64 2 to ptr), ptr %ptr10, align 8
  %ptr11 = getelementptr ptr, ptr %types_data, i64 11
  store ptr inttoptr (i64 2 to ptr), ptr %ptr11, align 8
  %ptr12 = getelementptr ptr, ptr %types_data, i64 12
  store ptr inttoptr (i64 2 to ptr), ptr %ptr12, align 8
  %ptr13 = getelementptr ptr, ptr %types_data, i64 13
  store ptr inttoptr (i64 2 to ptr), ptr %ptr13, align 8
  %ptr14 = getelementptr ptr, ptr %types_data, i64 14
  store ptr inttoptr (i64 2 to ptr), ptr %ptr14, align 8
  %ptr15 = getelementptr ptr, ptr %types_data, i64 15
  store ptr inttoptr (i64 2 to ptr), ptr %ptr15, align 8
  %ptr16 = getelementptr ptr, ptr %types_data, i64 16
  store ptr inttoptr (i64 2 to ptr), ptr %ptr16, align 8
  %ptr17 = getelementptr ptr, ptr %types_data, i64 17
  store ptr inttoptr (i64 2 to ptr), ptr %ptr17, align 8
  %ptr18 = getelementptr ptr, ptr %types_data, i64 18
  store ptr inttoptr (i64 2 to ptr), ptr %ptr18, align 8
  %ptr19 = getelementptr ptr, ptr %types_data, i64 19
  store ptr inttoptr (i64 2 to ptr), ptr %ptr19, align 8
  %ptr20 = getelementptr ptr, ptr %types_data, i64 20
  store ptr inttoptr (i64 1 to ptr), ptr %ptr20, align 8
  %ptr21 = getelementptr ptr, ptr %types_data, i64 21
  store ptr inttoptr (i64 3 to ptr), ptr %ptr21, align 8
  %ptr22 = getelementptr ptr, ptr %types_data, i64 22
  store ptr inttoptr (i64 3 to ptr), ptr %ptr22, align 8
  %ptr23 = getelementptr ptr, ptr %types_data, i64 23
  store ptr inttoptr (i64 3 to ptr), ptr %ptr23, align 8
  %ptr24 = getelementptr ptr, ptr %types_data, i64 24
  store ptr inttoptr (i64 3 to ptr), ptr %ptr24, align 8
  %ptr25 = getelementptr ptr, ptr %types_data, i64 25
  store ptr inttoptr (i64 3 to ptr), ptr %ptr25, align 8
  %ptr26 = getelementptr ptr, ptr %types_data, i64 26
  store ptr inttoptr (i64 3 to ptr), ptr %ptr26, align 8
  %ptr27 = getelementptr ptr, ptr %types_data, i64 27
  store ptr inttoptr (i64 3 to ptr), ptr %ptr27, align 8
  %ptr28 = getelementptr ptr, ptr %types_data, i64 28
  store ptr inttoptr (i64 3 to ptr), ptr %ptr28, align 8
  %ptr29 = getelementptr ptr, ptr %types_data, i64 29
  store ptr inttoptr (i64 3 to ptr), ptr %ptr29, align 8
  %ptr30 = getelementptr ptr, ptr %types_data, i64 30
  store ptr inttoptr (i64 3 to ptr), ptr %ptr30, align 8
  %ptr31 = getelementptr ptr, ptr %types_data, i64 31
  store ptr inttoptr (i64 3 to ptr), ptr %ptr31, align 8
  %ptr32 = getelementptr ptr, ptr %types_data, i64 32
  store ptr inttoptr (i64 3 to ptr), ptr %ptr32, align 8
  %ptr33 = getelementptr ptr, ptr %types_data, i64 33
  store ptr inttoptr (i64 3 to ptr), ptr %ptr33, align 8
  %ptr34 = getelementptr ptr, ptr %types_data, i64 34
  store ptr inttoptr (i64 3 to ptr), ptr %ptr34, align 8
  %ptr35 = getelementptr ptr, ptr %types_data, i64 35
  store ptr inttoptr (i64 3 to ptr), ptr %ptr35, align 8
  %ptr36 = getelementptr ptr, ptr %types_data, i64 36
  store ptr inttoptr (i64 3 to ptr), ptr %ptr36, align 8
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %types_header, i32 0, i32 0
  store i64 37, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %types_header, i32 0, i32 1
  store i64 37, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %types_header, i32 0, i32 2
  store ptr %types_data, ptr %2, align 8
  %df_ptr = call ptr @malloc(i64 24)
  %cols = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr, i32 0, i32 0
  %rows = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr, i32 0, i32 1
  %types = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr, i32 0, i32 2
  store ptr %names_header, ptr %cols, align 8
  store ptr %raw_rows_ptr, ptr %rows, align 8
  store ptr %types_header, ptr %types, align 8
  store ptr %df_ptr, ptr @df, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @ReadCsvInternal(ptr, ptr)

declare ptr @malloc(i64)
