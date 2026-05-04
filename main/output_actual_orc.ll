; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%struct_name_age = type { ptr, i64 }
%array = type { i64, i64, ptr }

@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@__result = global ptr null
@__i = global i64 0
@x = external global ptr
@d = external global ptr
@df_idx_err_msg = private unnamed_addr constant [64 x i8] c"Runtime Error: Cannot index dataframe (empty or out of bounds)\0A\00", align 1

define ptr @main_2() {
entry:
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 32)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 0, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 4, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %arr_header1 = call ptr @malloc(i64 24)
  %arr_data2 = call ptr @malloc(i64 32)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 0
  store i64 0, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 1
  store i64 4, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 2
  store ptr %arr_data2, ptr %5, align 8
  %df_data_raw = call ptr @malloc(i64 16)
  %data_gep = getelementptr ptr, ptr %df_data_raw, i64 0
  store ptr %arr_header, ptr %data_gep, align 8
  %data_gep3 = getelementptr ptr, ptr %df_data_raw, i64 1
  store ptr %arr_header1, ptr %data_gep3, align 8
  %df_data_header = call ptr @malloc(i64 24)
  %6 = getelementptr inbounds nuw { i64, i64, i64, ptr }, ptr %df_data_header, i32 0, i32 0
  store i64 2, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, i64, ptr }, ptr %df_data_header, i32 0, i32 1
  store i64 2, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, i64, ptr }, ptr %df_data_header, i32 0, i32 2
  store ptr %df_data_raw, ptr %8, align 8
  %arr_header4 = call ptr @malloc(i64 24)
  %arr_data5 = call ptr @malloc(i64 16)
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 0
  store i64 2, ptr %9, align 4
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 1
  store i64 2, ptr %10, align 4
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 2
  store ptr %arr_data5, ptr %11, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data5, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr6 = getelementptr ptr, ptr %arr_data5, i64 1
  store ptr @str.1, ptr %elem_ptr6, align 8
  %arr_header7 = call ptr @malloc(i64 24)
  %arr_data8 = call ptr @malloc(i64 16)
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 0
  store i64 2, ptr %12, align 4
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 1
  store i64 2, ptr %13, align 4
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 2
  store ptr %arr_data8, ptr %14, align 8
  %elem_ptr9 = getelementptr ptr, ptr %arr_data8, i64 0
  store ptr null, ptr %elem_ptr9, align 8
  %elem_ptr10 = getelementptr ptr, ptr %arr_data8, i64 1
  store ptr null, ptr %elem_ptr10, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %15 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header4, ptr %15, align 8
  %16 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %df_data_header, ptr %16, align 8
  %17 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header7, ptr %17, align 8
  store ptr %df, ptr @__result, align 8
  store i64 0, ptr @__i, align 8
  store i64 0, ptr @__i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__i_load = load i64, ptr @__i, align 4
  %x_load = load ptr, ptr @x, align 8
  %18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load, i32 0, i32 0
  %length = load i64, ptr %18, align 8
  %icmp_tmp = icmp slt i64 %__i_load, %length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %d_load = load ptr, ptr @d, align 8
  %ptr_age = getelementptr inbounds nuw %struct_name_age, ptr %d_load, i32 0, i32 1
  %load_age_ptr = load ptr, ptr %ptr_age, align 8
  %val_age = load i64, ptr %load_age_ptr, align 4
  %icmp_tmp11 = icmp sgt i64 %val_age, 25
  br i1 %icmp_tmp11, label %then, label %else

for.step:                                         ; preds = %ifcont
  %x_load18 = load i64, ptr @__i, align 8
  %inc_add = add i64 %x_load18, 1
  store i64 %inc_add, ptr @__i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__result_load19 = load ptr, ptr @__result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 2
  %data_ptr20 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__result_load19, ptr %data_ptr20, align 8
  ret ptr %runtime_obj

then:                                             ; preds = %for.body
  %__result_load = load ptr, ptr @__result, align 8
  %x_load12 = load ptr, ptr @x, align 8
  %__i_load13 = load i64, ptr @__i, align 4
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %x_load12, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr, align 4
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_empty = icmp eq i64 %len, 0
  %in_bounds = icmp ult i64 %__i_load13, %len
  %19 = xor i1 %in_bounds, true
  %invalid = or i1 %is_empty, %19
  br i1 %invalid, label %df_idx_err, label %df_idx_ok

else:                                             ; preds = %for.body
  br label %ifcont

ifcont:                                           ; preds = %else, %add_cont
  %iftmp = phi ptr [ %__result_load, %add_cont ], [ 0.000000e+00, %else ]
  br label %for.step

df_idx_ok:                                        ; preds = %then
  %elem_ptr14 = getelementptr ptr, ptr %data, i64 %__i_load13
  %record = load ptr, ptr %elem_ptr14, align 8
  br label %df_idx_merge

df_idx_err:                                       ; preds = %then
  %print_err = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg)
  br label %df_idx_merge

df_idx_merge:                                     ; preds = %df_idx_ok, %df_idx_err
  %df_idx_result = phi ptr [ null, %df_idx_err ], [ %record, %df_idx_ok ]
  %len_ptr15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__result_load, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__result_load, i32 0, i32 1
  %data_ptr_ptr16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__result_load, i32 0, i32 2
  %len17 = load i64, ptr %len_ptr15, align 4
  %cap = load i64, ptr %cap_ptr, align 4
  %data_ptr = load ptr, ptr %data_ptr_ptr16, align 8
  %is_full = icmp eq i64 %len17, %cap
  br i1 %is_full, label %grow, label %add_cont

grow:                                             ; preds = %df_idx_merge
  %20 = icmp eq i64 %cap, 0
  %21 = mul i64 %cap, 2
  %new_cap = select i1 %20, i64 4, i64 %21
  %new_byte_size = mul i64 %new_cap, 8
  %realloc_ptr = call ptr @realloc(ptr %data_ptr, i64 %new_byte_size)
  store i64 %new_cap, ptr %cap_ptr, align 4
  store ptr %realloc_ptr, ptr %data_ptr_ptr16, align 8
  br label %add_cont

add_cont:                                         ; preds = %grow, %df_idx_merge
  %final_data_ptr = phi ptr [ %data_ptr, %df_idx_merge ], [ %realloc_ptr, %grow ]
  %target_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len17
  store ptr %df_idx_result, ptr %target_ptr, align 8
  %22 = add i64 %len17, 1
  store i64 %22, ptr %len_ptr15, align 4
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
