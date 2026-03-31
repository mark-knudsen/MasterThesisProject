; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr, ptr }

@str = private unnamed_addr constant [4 x i8] c"dan\00", align 1
@str.1 = private unnamed_addr constant [6 x i8] c"alice\00", align 1
@str.2 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.3 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.4 = private unnamed_addr constant [7 x i8] c"string\00", align 1
@str.5 = private unnamed_addr constant [4 x i8] c"int\00", align 1

define ptr @main_3() {
entry:
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 16)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 2, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 2, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %arr_header2 = call ptr @malloc(i64 24)
  %arr_data3 = call ptr @malloc(i64 16)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 0
  store i64 2, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 1
  store i64 2, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 2
  store ptr %arr_data3, ptr %5, align 8
  %elem_ptr4 = getelementptr ptr, ptr %arr_data3, i64 0
  store ptr bitcast (i64 30 to ptr), ptr %elem_ptr4, align 8
  %elem_ptr5 = getelementptr ptr, ptr %arr_data3, i64 1
  store ptr bitcast (i64 25 to ptr), ptr %elem_ptr5, align 8
  %df_data_raw = call ptr @malloc(i64 16)
  %data_gep = getelementptr ptr, ptr %df_data_raw, i64 0
  store ptr %arr_header, ptr %data_gep, align 8
  %data_gep6 = getelementptr ptr, ptr %df_data_raw, i64 1
  store ptr %arr_header2, ptr %data_gep6, align 8
  %df_data_header = call ptr @malloc(i64 24)
  %6 = getelementptr inbounds nuw { i64, i64, i64, ptr }, ptr %df_data_header, i32 0, i32 0
  store i64 2, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, i64, ptr }, ptr %df_data_header, i32 0, i32 1
  store i64 2, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, i64, ptr }, ptr %df_data_header, i32 0, i32 2
  store ptr %df_data_raw, ptr %8, align 8
  %arr_header7 = call ptr @malloc(i64 24)
  %arr_data8 = call ptr @malloc(i64 16)
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 0
  store i64 2, ptr %9, align 4
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 1
  store i64 2, ptr %10, align 4
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 2
  store ptr %arr_data8, ptr %11, align 8
  %elem_ptr9 = getelementptr ptr, ptr %arr_data8, i64 0
  store ptr null, ptr %elem_ptr9, align 8
  %elem_ptr10 = getelementptr ptr, ptr %arr_data8, i64 1
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr10, align 8
  %12 = getelementptr inbounds nuw { i64, i64, i64, ptr }, ptr %df_data_header, i32 0, i32 3
  store ptr %arr_header7, ptr %12, align 8
  %arr_header11 = call ptr @malloc(i64 24)
  %arr_data12 = call ptr @malloc(i64 16)
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header11, i32 0, i32 0
  store i64 2, ptr %13, align 4
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header11, i32 0, i32 1
  store i64 2, ptr %14, align 4
  %15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header11, i32 0, i32 2
  store ptr %arr_data12, ptr %15, align 8
  %elem_ptr13 = getelementptr ptr, ptr %arr_data12, i64 0
  store ptr @str.2, ptr %elem_ptr13, align 8
  %elem_ptr14 = getelementptr ptr, ptr %arr_data12, i64 1
  store ptr @str.3, ptr %elem_ptr14, align 8
  %arr_header15 = call ptr @malloc(i64 24)
  %arr_data16 = call ptr @malloc(i64 16)
  %16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 0
  store i64 2, ptr %16, align 4
  %17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 1
  store i64 2, ptr %17, align 4
  %18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 2
  store ptr %arr_data16, ptr %18, align 8
  %elem_ptr17 = getelementptr ptr, ptr %arr_data16, i64 0
  store ptr @str.4, ptr %elem_ptr17, align 8
  %elem_ptr18 = getelementptr ptr, ptr %arr_data16, i64 1
  store ptr @str.5, ptr %elem_ptr18, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %19 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header11, ptr %19, align 8
  %20 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %df_data_header, ptr %20, align 8
  %21 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header15, ptr %21, align 8
  %22 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 3
  store ptr %arr_header7, ptr %22, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %df, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
