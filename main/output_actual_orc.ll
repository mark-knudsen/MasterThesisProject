; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@df = external global ptr
@__map_src = external global ptr, align 8
@__map_result = external global ptr, align 8
@__map_i = external global i64, align 8
@df_idx_err_msg = private unnamed_addr constant [64 x i8] c"Runtime Error: Cannot index dataframe (empty or out of bounds)\0A\00", align 1
@str = private unnamed_addr constant [5 x i8] c"test\00", align 1
@df_idx_err_msg.1 = private unnamed_addr constant [64 x i8] c"Runtime Error: Cannot index dataframe (empty or out of bounds)\0A\00", align 1
@str.2 = private unnamed_addr constant [5 x i8] c"test\00", align 1

define ptr @main_4() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 800)
  %arr_data = bitcast ptr %arr_data_raw to ptr
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %0, align 4
  store i64 100, ptr %1, align 4
  store ptr %arr_data, ptr %2, align 8
  store ptr %arr_header, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %rows_length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %__map_src_load1 = load ptr, ptr @__map_src, align 8
  %__map_i_load2 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load1, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr, align 4
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_empty = icmp eq i64 %len, 0
  %in_bounds = icmp ult i64 %__map_i_load2, %len
  %3 = xor i1 %in_bounds, true
  %invalid = or i1 %is_empty, %3
  br i1 %invalid, label %df_idx_err, label %df_idx_ok

for.step:                                         ; preds = %cont
  %x_load = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__map_result_load65 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__map_result_load65, ptr %data_ptr, align 8
  ret ptr %runtime_obj

df_idx_ok:                                        ; preds = %for.body
  %elem_ptr = getelementptr ptr, ptr %data, i64 %__map_i_load2
  %record = load ptr, ptr %elem_ptr, align 8
  br label %df_idx_merge

df_idx_err:                                       ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg)
  br label %df_idx_merge

df_idx_merge:                                     ; preds = %df_idx_ok, %df_idx_err
  %df_idx_result = phi ptr [ null, %df_idx_err ], [ %record, %df_idx_ok ]
  %record_buffer = call ptr @malloc(i64 8)
  %record_slots = bitcast ptr %record_buffer to ptr
  %field_ptr = getelementptr ptr, ptr %record_slots, i64 0
  store ptr @str, ptr %field_ptr, align 8
  %__map_src_load3 = load ptr, ptr @__map_src, align 8
  %__map_i_load4 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr5 = getelementptr inbounds nuw %dataframe, ptr %__map_src_load3, i32 0, i32 1
  %rows6 = load ptr, ptr %rows_ptr_ptr5, align 8
  %len_ptr7 = getelementptr inbounds nuw %array, ptr %rows6, i32 0, i32 0
  %len8 = load i64, ptr %len_ptr7, align 4
  %data_ptr_ptr9 = getelementptr inbounds nuw %array, ptr %rows6, i32 0, i32 2
  %data10 = load ptr, ptr %data_ptr_ptr9, align 8
  %is_empty11 = icmp eq i64 %len8, 0
  %in_bounds12 = icmp ult i64 %__map_i_load4, %len8
  %4 = xor i1 %in_bounds12, true
  %invalid13 = or i1 %is_empty11, %4
  br i1 %invalid13, label %df_idx_err15, label %df_idx_ok14

df_idx_ok14:                                      ; preds = %df_idx_merge
  %elem_ptr18 = getelementptr ptr, ptr %data10, i64 %__map_i_load4
  %record19 = load ptr, ptr %elem_ptr18, align 8
  br label %df_idx_merge16

df_idx_err15:                                     ; preds = %df_idx_merge
  %print_err17 = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg.1)
  br label %df_idx_merge16

df_idx_merge16:                                   ; preds = %df_idx_ok14, %df_idx_err15
  %df_idx_result20 = phi ptr [ null, %df_idx_err15 ], [ %record19, %df_idx_ok14 ]
  %record_buffer21 = call ptr @malloc(i64 8)
  %record_slots22 = bitcast ptr %record_buffer21 to ptr
  %field_ptr23 = getelementptr ptr, ptr %record_slots22, i64 0
  store ptr @str.2, ptr %field_ptr23, align 8
  %5 = getelementptr ptr, ptr %df_idx_result20, i64 0
  %loaded_field = load ptr, ptr %5, align 8
  %6 = getelementptr ptr, ptr %df_idx_result20, i64 1
  %loaded_field24 = load ptr, ptr %6, align 8
  %7 = getelementptr ptr, ptr %df_idx_result20, i64 2
  %loaded_field25 = load ptr, ptr %7, align 8
  %8 = getelementptr ptr, ptr %df_idx_result20, i64 3
  %loaded_field26 = load ptr, ptr %8, align 8
  %9 = getelementptr ptr, ptr %df_idx_result20, i64 4
  %loaded_field27 = load ptr, ptr %9, align 8
  %10 = getelementptr ptr, ptr %df_idx_result20, i64 5
  %loaded_field28 = load ptr, ptr %10, align 8
  %11 = getelementptr ptr, ptr %df_idx_result20, i64 6
  %loaded_field29 = load ptr, ptr %11, align 8
  %12 = getelementptr ptr, ptr %df_idx_result20, i64 7
  %loaded_field30 = load ptr, ptr %12, align 8
  %13 = getelementptr ptr, ptr %df_idx_result20, i64 8
  %loaded_field31 = load ptr, ptr %13, align 8
  %14 = getelementptr ptr, ptr %df_idx_result20, i64 9
  %loaded_field32 = load ptr, ptr %14, align 8
  %15 = getelementptr ptr, ptr %df_idx_result20, i64 10
  %loaded_field33 = load ptr, ptr %15, align 8
  %16 = getelementptr ptr, ptr %df_idx_result20, i64 11
  %loaded_field34 = load ptr, ptr %16, align 8
  %17 = getelementptr ptr, ptr %df_idx_result20, i64 12
  %loaded_field35 = load ptr, ptr %17, align 8
  %18 = getelementptr ptr, ptr %df_idx_result20, i64 13
  %loaded_field36 = load ptr, ptr %18, align 8
  %19 = getelementptr ptr, ptr %df_idx_result20, i64 14
  %loaded_field37 = load ptr, ptr %19, align 8
  %20 = getelementptr ptr, ptr %df_idx_result20, i64 15
  %loaded_field38 = load ptr, ptr %20, align 8
  %21 = getelementptr ptr, ptr %df_idx_result20, i64 16
  %loaded_field39 = load ptr, ptr %21, align 8
  %22 = getelementptr ptr, ptr %df_idx_result20, i64 17
  %loaded_field40 = load ptr, ptr %22, align 8
  %23 = getelementptr ptr, ptr %df_idx_result20, i64 18
  %loaded_field41 = load ptr, ptr %23, align 8
  %24 = getelementptr ptr, ptr %df_idx_result20, i64 19
  %loaded_field42 = load ptr, ptr %24, align 8
  %25 = getelementptr ptr, ptr %df_idx_result20, i64 20
  %loaded_field43 = load ptr, ptr %25, align 8
  %26 = getelementptr ptr, ptr %df_idx_result20, i64 21
  %loaded_field44 = load ptr, ptr %26, align 8
  %27 = getelementptr ptr, ptr %df_idx_result20, i64 22
  %loaded_field45 = load ptr, ptr %27, align 8
  %28 = getelementptr ptr, ptr %df_idx_result20, i64 23
  %loaded_field46 = load ptr, ptr %28, align 8
  %29 = getelementptr ptr, ptr %df_idx_result20, i64 24
  %loaded_field47 = load ptr, ptr %29, align 8
  %30 = getelementptr ptr, ptr %df_idx_result20, i64 25
  %loaded_field48 = load ptr, ptr %30, align 8
  %31 = getelementptr ptr, ptr %df_idx_result20, i64 26
  %loaded_field49 = load ptr, ptr %31, align 8
  %32 = getelementptr ptr, ptr %df_idx_result20, i64 27
  %loaded_field50 = load ptr, ptr %32, align 8
  %33 = getelementptr ptr, ptr %df_idx_result20, i64 28
  %loaded_field51 = load ptr, ptr %33, align 8
  %34 = getelementptr ptr, ptr %df_idx_result20, i64 29
  %loaded_field52 = load ptr, ptr %34, align 8
  %35 = getelementptr ptr, ptr %df_idx_result20, i64 30
  %loaded_field53 = load ptr, ptr %35, align 8
  %36 = getelementptr ptr, ptr %df_idx_result20, i64 31
  %loaded_field54 = load ptr, ptr %36, align 8
  %37 = getelementptr ptr, ptr %df_idx_result20, i64 32
  %loaded_field55 = load ptr, ptr %37, align 8
  %38 = getelementptr ptr, ptr %df_idx_result20, i64 33
  %loaded_field56 = load ptr, ptr %38, align 8
  %39 = getelementptr ptr, ptr %df_idx_result20, i64 34
  %loaded_field57 = load ptr, ptr %39, align 8
  %40 = getelementptr ptr, ptr %df_idx_result20, i64 35
  %loaded_field58 = load ptr, ptr %40, align 8
  %41 = getelementptr ptr, ptr %df_idx_result20, i64 36
  %loaded_field59 = load ptr, ptr %41, align 8
  %42 = getelementptr ptr, ptr %record_buffer21, i64 0
  %loaded_field60 = load ptr, ptr %42, align 8
  %rec_ptr = call ptr @malloc(i64 304)
  %43 = getelementptr ptr, ptr %rec_ptr, i64 0
  store ptr %loaded_field, ptr %43, align 8
  %44 = getelementptr ptr, ptr %rec_ptr, i64 1
  store ptr %loaded_field24, ptr %44, align 8
  %45 = getelementptr ptr, ptr %rec_ptr, i64 2
  store ptr %loaded_field25, ptr %45, align 8
  %46 = getelementptr ptr, ptr %rec_ptr, i64 3
  store ptr %loaded_field26, ptr %46, align 8
  %47 = getelementptr ptr, ptr %rec_ptr, i64 4
  store ptr %loaded_field27, ptr %47, align 8
  %48 = getelementptr ptr, ptr %rec_ptr, i64 5
  store ptr %loaded_field28, ptr %48, align 8
  %49 = getelementptr ptr, ptr %rec_ptr, i64 6
  store ptr %loaded_field29, ptr %49, align 8
  %50 = getelementptr ptr, ptr %rec_ptr, i64 7
  store ptr %loaded_field30, ptr %50, align 8
  %51 = getelementptr ptr, ptr %rec_ptr, i64 8
  store ptr %loaded_field31, ptr %51, align 8
  %52 = getelementptr ptr, ptr %rec_ptr, i64 9
  store ptr %loaded_field32, ptr %52, align 8
  %53 = getelementptr ptr, ptr %rec_ptr, i64 10
  store ptr %loaded_field33, ptr %53, align 8
  %54 = getelementptr ptr, ptr %rec_ptr, i64 11
  store ptr %loaded_field34, ptr %54, align 8
  %55 = getelementptr ptr, ptr %rec_ptr, i64 12
  store ptr %loaded_field35, ptr %55, align 8
  %56 = getelementptr ptr, ptr %rec_ptr, i64 13
  store ptr %loaded_field36, ptr %56, align 8
  %57 = getelementptr ptr, ptr %rec_ptr, i64 14
  store ptr %loaded_field37, ptr %57, align 8
  %58 = getelementptr ptr, ptr %rec_ptr, i64 15
  store ptr %loaded_field38, ptr %58, align 8
  %59 = getelementptr ptr, ptr %rec_ptr, i64 16
  store ptr %loaded_field39, ptr %59, align 8
  %60 = getelementptr ptr, ptr %rec_ptr, i64 17
  store ptr %loaded_field40, ptr %60, align 8
  %61 = getelementptr ptr, ptr %rec_ptr, i64 18
  store ptr %loaded_field41, ptr %61, align 8
  %62 = getelementptr ptr, ptr %rec_ptr, i64 19
  store ptr %loaded_field42, ptr %62, align 8
  %63 = getelementptr ptr, ptr %rec_ptr, i64 20
  store ptr %loaded_field43, ptr %63, align 8
  %64 = getelementptr ptr, ptr %rec_ptr, i64 21
  store ptr %loaded_field44, ptr %64, align 8
  %65 = getelementptr ptr, ptr %rec_ptr, i64 22
  store ptr %loaded_field45, ptr %65, align 8
  %66 = getelementptr ptr, ptr %rec_ptr, i64 23
  store ptr %loaded_field46, ptr %66, align 8
  %67 = getelementptr ptr, ptr %rec_ptr, i64 24
  store ptr %loaded_field47, ptr %67, align 8
  %68 = getelementptr ptr, ptr %rec_ptr, i64 25
  store ptr %loaded_field48, ptr %68, align 8
  %69 = getelementptr ptr, ptr %rec_ptr, i64 26
  store ptr %loaded_field49, ptr %69, align 8
  %70 = getelementptr ptr, ptr %rec_ptr, i64 27
  store ptr %loaded_field50, ptr %70, align 8
  %71 = getelementptr ptr, ptr %rec_ptr, i64 28
  store ptr %loaded_field51, ptr %71, align 8
  %72 = getelementptr ptr, ptr %rec_ptr, i64 29
  store ptr %loaded_field52, ptr %72, align 8
  %73 = getelementptr ptr, ptr %rec_ptr, i64 30
  store ptr %loaded_field53, ptr %73, align 8
  %74 = getelementptr ptr, ptr %rec_ptr, i64 31
  store ptr %loaded_field54, ptr %74, align 8
  %75 = getelementptr ptr, ptr %rec_ptr, i64 32
  store ptr %loaded_field55, ptr %75, align 8
  %76 = getelementptr ptr, ptr %rec_ptr, i64 33
  store ptr %loaded_field56, ptr %76, align 8
  %77 = getelementptr ptr, ptr %rec_ptr, i64 34
  store ptr %loaded_field57, ptr %77, align 8
  %78 = getelementptr ptr, ptr %rec_ptr, i64 35
  store ptr %loaded_field58, ptr %78, align 8
  %79 = getelementptr ptr, ptr %rec_ptr, i64 36
  store ptr %loaded_field59, ptr %79, align 8
  %80 = getelementptr ptr, ptr %rec_ptr, i64 37
  store ptr %loaded_field60, ptr %80, align 8
  %len_ptr61 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 1
  %data_ptr_ptr62 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 2
  %len63 = load i64, ptr %len_ptr61, align 8
  %cap = load i64, ptr %cap_ptr, align 8
  %data64 = load ptr, ptr %data_ptr_ptr62, align 8
  %is_full = icmp uge i64 %len63, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %df_idx_merge16
  %81 = icmp eq i64 %cap, 0
  %82 = mul i64 %cap, 2
  %new_cap = select i1 %81, i64 4, i64 %82
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data64, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr, align 8
  store ptr %realloc, ptr %data_ptr_ptr62, align 8
  br label %cont

cont:                                             ; preds = %grow, %df_idx_merge16
  %final_data_ptr = phi ptr [ %data64, %df_idx_merge16 ], [ %realloc, %grow ]
  %target = getelementptr ptr, ptr %final_data_ptr, i64 %len63
  %83 = bitcast ptr %rec_ptr to ptr
  store ptr %83, ptr %target, align 8
  %new_len = add i64 %len63, 1
  store i64 %new_len, ptr %len_ptr61, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
