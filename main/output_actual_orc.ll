; ModuleID = 'repl_module'
source_filename = "repl_module"

@str = private unnamed_addr constant [13 x i8] c"CSV/test.csv\00", align 1
@csv_schema_code = private unnamed_addr constant [5 x i8] c"SIBF\00", align 1
@col_name_0 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@col_name_1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@col_name_2 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@col_name_3 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@df = global ptr null, align 8

define ptr @main_0() {
entry:
  %csv_boxed_res = call ptr @ReadCsvInternal(ptr @str, ptr @csv_schema_code)
  %unbox_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %csv_boxed_res, i32 0, i32 1
  %raw_rows_ptr = load ptr, ptr %unbox_ptr, align 8
  %names_header = call ptr @malloc(i64 24)
  %names_data = call ptr @malloc(i64 32)
  %ptr_0 = getelementptr ptr, ptr %names_data, i64 0
  store ptr @col_name_0, ptr %ptr_0, align 8
  %ptr_1 = getelementptr ptr, ptr %names_data, i64 1
  store ptr @col_name_1, ptr %ptr_1, align 8
  %ptr_2 = getelementptr ptr, ptr %names_data, i64 2
  store ptr @col_name_2, ptr %ptr_2, align 8
  %ptr_3 = getelementptr ptr, ptr %names_data, i64 3
  store ptr @col_name_3, ptr %ptr_3, align 8
  %len = getelementptr inbounds nuw { i64, i64, ptr }, ptr %names_header, i32 0, i32 0
  store i64 4, ptr %len, align 4
  %cap = getelementptr inbounds nuw { i64, i64, ptr }, ptr %names_header, i32 0, i32 1
  store i64 4, ptr %cap, align 4
  %data = getelementptr inbounds nuw { i64, i64, ptr }, ptr %names_header, i32 0, i32 2
  store ptr %names_data, ptr %data, align 8
  %types_header = call ptr @malloc(i64 24)
  %types_data = call ptr @malloc(i64 32)
  %ptr = getelementptr ptr, ptr %types_data, i64 0
  store ptr inttoptr (i64 4 to ptr), ptr %ptr, align 8
  %ptr1 = getelementptr ptr, ptr %types_data, i64 1
  store ptr inttoptr (i64 1 to ptr), ptr %ptr1, align 8
  %ptr2 = getelementptr ptr, ptr %types_data, i64 2
  store ptr inttoptr (i64 3 to ptr), ptr %ptr2, align 8
  %ptr3 = getelementptr ptr, ptr %types_data, i64 3
  store ptr inttoptr (i64 2 to ptr), ptr %ptr3, align 8
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %types_header, i32 0, i32 0
  store i64 4, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %types_header, i32 0, i32 1
  store i64 4, ptr %1, align 4
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
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @ReadCsvInternal(ptr, ptr)

declare ptr @malloc(i64)
