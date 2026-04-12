; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1

define ptr @main_5() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %0 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %x_load, i32 0, i32 1
  %src_rows_ptr = load ptr, ptr %0, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %src_rows_ptr, i32 0, i32 0
  %row_count = load i64, ptr %len_ptr, align 4
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 160000)
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 1, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 20000, ptr %2, align 4
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %3, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %arr_header1 = call ptr @malloc(i64 24)
  %arr_data2 = call ptr @malloc(i64 160000)
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 0
  store i64 1, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 1
  store i64 20000, ptr %5, align 4
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 2
  store ptr %arr_data2, ptr %6, align 8
  %elem_ptr3 = getelementptr ptr, ptr %arr_data2, i64 0
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr3, align 8
  %arr_header4 = call ptr @malloc(i64 24)
  %data_size = mul i64 %row_count, 8
  %arr_data5 = call ptr @malloc(i64 %data_size)
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 0
  store i64 %row_count, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 1
  store i64 %row_count, ptr %8, align 4
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 2
  store ptr %arr_data5, ptr %9, align 8
  %"$row_slot" = alloca ptr, align 8
  %index_ptr = alloca i64, align 8
  store i64 0, ptr %index_ptr, align 4
  br label %loop_cond

loop_cond:                                        ; preds = %loop_body, %entry
  %index = load i64, ptr %index_ptr, align 4
  %cond = icmp slt i64 %index, %row_count
  br i1 %cond, label %loop_body, label %loop_end

loop_body:                                        ; preds = %loop_cond
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %src_rows_ptr, i32 0, i32 2
  %src_data = load ptr, ptr %10, align 8
  %11 = getelementptr ptr, ptr %src_data, i64 %index
  %old_rec = load ptr, ptr %11, align 8
  store ptr %old_rec, ptr %"$row_slot", align 8
  %"$row_load" = load ptr, ptr %"$row_slot", align 8
  %ptr_name = getelementptr ptr, ptr %"$row_load", i64 1
  %load_name_ptr = load ptr, ptr %ptr_name, align 8
  %rec_ptr = call ptr @malloc(i64 8)
  %12 = getelementptr ptr, ptr %rec_ptr, i64 0
  store ptr %load_name_ptr, ptr %12, align 8
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 2
  %res_data = load ptr, ptr %13, align 8
  %14 = getelementptr ptr, ptr %res_data, i64 %index
  store ptr %rec_ptr, ptr %14, align 8
  %next_index = add i64 %index, 1
  store i64 %next_index, ptr %index_ptr, align 4
  br label %loop_cond

loop_end:                                         ; preds = %loop_cond
  %df_show = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr ({ ptr, ptr, ptr }, ptr null, i32 1) to i32))
  %15 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_show, i32 0, i32 0
  store ptr %arr_header, ptr %15, align 8
  %16 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_show, i32 0, i32 1
  store ptr %arr_header4, ptr %16, align 8
  %17 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_show, i32 0, i32 2
  store ptr %arr_header1, ptr %17, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr %df_show, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
